pipeline {
    agent {
        label 'Jenkins-Windows-2019'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'5'))
    }
    parameters {
        string(name: 'branch', defaultValue: 'master', description: 'Branch to build')
        string(name: 'version', defaultValue: '5.2.6', description: 'Branch to build')
        booleanParam(name: 'deploy', defaultValue: true, description: 'Should deploy on STG')
    }
    environment {
        SSH_KEY = "/c/keys/OTT-STG.pem"
        PRE_PROD_SERVER = "ubuntu@ec2-34-253-153-26.eu-west-1.compute.amazonaws.com"
    }
    stages {
        stage("Checkout SCM"){
            steps{
                dir('TvmCore'){ git(url: 'git@github.com:kaltura/tvmcore.git', branch: "${params.branch}") }
                dir('tvpapi_rest'){ git(url: 'git@github.com:kaltura/Phoenix.git', branch: "${params.branch}") }
                dir('Core'){ git(url: 'git@github.com:kaltura/Core.git', branch: "${params.branch}") }
                dir('CDNTokenizers'){ git(url: 'git@github.com:kaltura/CDNTokenizers.git', branch: "${params.branch}") }
                dir('tvincicommon'){ git(url: 'git@github.com:kaltura/TvinciCommon.git', branch: "${params.branch}") }
                dir('remotetasks'){ git(url: 'git@github.com:kaltura/RemoteTasks.git', branch: "${params.branch}") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("Core"){
                    bat "sh DllVersioning.Core.sh ." 
                    bat "sh DllVersioning.Core.sh ../TvmCore" 
                    bat "sh DllVersioning.Core.sh ../remotetasks" 
                }
            }        
        }
        stage("Publish Projects"){
            steps{
                echo "Cleanning Nugets dir before packaging new nugets"
                dir("published"){ deleteDir() }

                bat "dotnet publish remotetasks/IngestHandler/IngestHandler.csproj -o ${WORKSPACE}/published/ingest-handler" 
                bat "dotnet publish remotetasks/IngestTransformationHandler/IngestTransformationHandler.csproj -o ${WORKSPACE}/published/ingest-transformation-handler" 
                bat "dotnet publish remotetasks/IngestValidtionHandler/IngestValidtionHandler.csproj -o ${WORKSPACE}/published/ingest-validation-handler" 
                
            }        
        }
        stage("Zip and Release"){
            steps { 
                dir("published"){ 
                    bat "7z.exe a -r remotetasks_${params.version}.zip *"
                    bat "xcopy /y remotetasks_${params.version}.zip c:\\version_release\\RemoteTasks\\${params.branch}\\"
                }
            }
        }
        stage("Deploy"){
            steps { 
                dir("published"){ 
                    sh "scp -i ${SSH_KEY} remotetasks_${params.version}.zip ${PRE_PROD_SERVER}:/home/ubuntu/remotetasks_${params.version}.zip"
                    sh "ssh -i ${SSH_KEY} ${PRE_PROD_SERVER} \"sudo systemctl stop kaltura-ingest-handler.service; sudo systemctl stop kaltura-ingest-transformation-handler.service; sudo systemctl stop kaltura-ingest-validation-handler.service\""
                    sh "ssh -i ${SSH_KEY} ${PRE_PROD_SERVER} \"sudo rm -rf remote-tasks\""
                    sh "ssh -i ${SSH_KEY} ${PRE_PROD_SERVER} \"unzip remotetasks_${params.version}.zip -d remote-tasks\""
                    sh "ssh -i ${SSH_KEY} ${PRE_PROD_SERVER} \"sudo systemctl daemon-reload\""
                    sh "ssh -i ${SSH_KEY} ${PRE_PROD_SERVER} \"sudo systemctl start kaltura-ingest-handler.service; sudo systemctl start kaltura-ingest-transformation-handler.service; sudo systemctl start kaltura-ingest-validation-handler.service\""
                }
            }
        }
    }
    post {
        always {
            emailext (
                subject: "Jenkins Build ${currentBuild.currentResult}: Job ${env.JOB_NAME}",
                to: "arthur.vaverko@kaltura.com",
                mimeType : "text/html",
                body: "${currentBuild.currentResult}: Job ${env.JOB_NAME}\nbuild:<a href='${env.BUILD_URL}'>${env.BUILD_NUMBER}</a>\nPath:\\\\34.252.63.117\\version_release\\RemoteTasks\\${params.branch}\\remotetasks_${params.version}.zip \n"
            )

        }
    }
}