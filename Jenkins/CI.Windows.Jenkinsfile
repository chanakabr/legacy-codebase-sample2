pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'50'))
        skipDefaultCheckout true
    }
    environment {
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
    }
    parameters {
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch')
        booleanParam(name: 'TRIGGER_RC', defaultValue: true, description: 'Should trigger Release Candidate?')
        booleanParam(name: 'publish', defaultValue: false, description: 'Publush api client libs ?')
    }
    stages {
        stage("Checkout"){
            steps{
                cleanWs()
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi_rest') { git(url: 'https://github.com/kaltura/Phoenix.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }

                script{
                    withCredentials([string(credentialsId: 'github-ott-ci-cd-token', variable: 'TOKEN')]) {
                        def getDefaultBranchCmd = "curl --silent -i ott-ci-cd:${TOKEN} https://api.github.com/repos/kaltura/clients-generator | grep 'default_branch' | cut -f4 -d'\"'"
                        def defaultGeneratorBranch = sh(label:"Get Default Branch From Github", script: getDefaultBranchCmd, , returnStdout: true).trim()
                        echo("Identified default clients-generator branch as: [${defaultGeneratorBranch}]")
                        dir('clients-generator'){ git(url: 'https://github.com/kaltura/clients-generator.git', branch:"${defaultGeneratorBranch}", credentialsId: "github-ott-ci-cd") }
                    }
                }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("tvpapi_rest") { 
                    bat "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("nuget restore") }
                dir("tvpapi_rest"){ bat ("nuget restore") }
            }        
        }
        stage("Clean"){
            steps{
                sh(label:"clean bin and obj folders", script:"find . -iname 'bin' -o -iname 'obj' | xargs rm -rf")
                // sh(label:"clean published folder", script:"rm -rf ./published")
            }
        }
        stage("Build"){
            environment{
                TCM_URL="http://tcm.service.consul:8080"
                TCM_APP="OTT_API_SV"
            }
            steps{
                dir("tvpapi_rest"){
                    bat (label:"Run MSBuild Phoenix" , script:"\"${MSBUILD}\" Phoenix.Legacy\\Phoenix.Legacy.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/kaltura_ott_api/"
                    )
                }
            }        
        }
        stage("Generate KalturaClient.xml"){
            steps { 
                dir("tvpapi_rest/Generator"){
                    bat (label:"Build Generator", script:"\"${MSBUILD}\" -p:Configuration=Release -m:4 -nr:False -t:Restore,Build")
                    dir("bin/Release/netcoreapp3.0"){
                        bat (label:"Generate KalturaClient.xml", script:"dotnet Generator.dll")
                        bat (label:"Copy KalturaClient.xml to clientlib folder", script:"xcopy KalturaClient.xml ${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\")
                    }
                }
            }
        }
        stage("Generate Kaltura Clients"){
            
            steps { 
                dir("clients-generator"){
                    bat ("php exec.php --dont-gzip -x${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\KalturaClient.xml "
                        +"-tott java,node,ngx,php5Zend,csharp,python,ajax,android,typescript,swift,php5,php53 " 
                        +"${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\"
                    )

                    // AFAIK WE dont use test-me anymore we use phoenix docs
                    // bat ("php exec.php --dont-gzip -x${WORKSPACE}\\published\\kaltura_ott_api\\testme\\KalturaClient.xml "
                    //     +"-tott ottTestme,testmeDoc"
                    //     +"${WORKSPACE}\\published\\kaltura_ott_api\\testme\\"
                    // )
                }
            }
        }
        stage("Publish Kaltura Clients"){
            // Generate only when release branch
            when {
                expression { return BRANCH_NAME =~ /\d+_\d+_\d+$/ || BRANCH_NAME == 'master' || params.publish == true }     
            }
            steps{
                dir("clients-generator"){
                    withCredentials([string(credentialsId: 'github-ott-ci-cd-token', variable: 'TOKEN')]) {
                        nodejs(nodeJSInstallationName: 'default') {
                            sh (
                                label: "Configure git user and run node script to push client lib updates", 
                                script: """
                                    git config --global user.email "ott.rnd.core@kaltura.com"
                                    git config --global user.name "Backend CI"
                                    npm install
                                    node copyAndPush '${WORKSPACE}/published/kaltura_ott_api/clientlibs' '${BRANCH_NAME}' '${TOKEN}' 'git'
                                """
                            )
                        }
                    }
                }
            }
        }
        stage("Zip and Publish"){
            environment {
                    RELEASE_FULL_VERSION = sh(label:"Extract Full Verion Tag", script: 'cd tvpapi_rest && ../Core/get-version-tag.sh', , returnStdout: true).trim()
                    RELEASE_MAIN_VERSION = sh(label:"Extract Main Version Tag", script: "echo $version | sed -e 's/\\.[0-9]*\$//g'", , returnStdout: true).trim();
            }
            steps{
                dir("published"){
                    sh (script: "echo 'buildnum ${BUILD_NUMBER}' > kaltura_ott_api/version.txt")
                    sh (script: "cd ../tvpapi_rest && git rev-parse HEAD >> ../published/kaltura_ott_api/version.txt")
                    bat (label:"Zip Artifacts", script:"7z.exe a -r phoenix.zip *")
                    sh (label:"upload to s3", script:"aws s3 cp phoenix.zip s3://${S3_BUILD_BUCKET_NAME}/mediahub/${BRANCH_NAME}/build/phoenix.zip")
                }
            }        
        }
        stage("Trigger Release Candidate"){
            when { expression { params.TRIGGER_RC == true } }
            steps{
                build (
                    job: "OTT-BE-Create-Release-Candidate", 
                    wait: false,
                    parameters: [
                        [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                    ]
                )
            }
        }
    }
    post {
        always {
            report()
        }
    }
}

def report(){
    configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
    def GIT_COMMIT = sh(label:"Obtain GIT Commit", script: "cd tvpapi_rest && git rev-parse HEAD", returnStdout: true).trim();
    def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} build ${env.BUILD_NUMBER} ${env.JOB_NAME} build ${currentBuild.currentResult} ${GIT_COMMIT} NA", returnStdout: true)
    echo "${report}"
    // return report
}
