pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    environment {
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
    }
    stages {
        stage("Checkout"){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvmcore') { git(url: 'https://github.com/kaltura/tvmcore.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('cdntokenizers') { git(url: 'https://github.com/kaltura/CDNTokenizers.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('ws_ingest') { git(url: 'https://github.com/kaltura/WS_Ingest.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("tvmcore") { bat "sh ../core/DllVersioning.Core.sh ." }
                dir("cdntokenizers") { bat "sh ../core/DllVersioning.Core.sh ." }
                dir("ws_ingest") { bat "sh ../core/DllVersioning.Core.sh ." }
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("nuget restore Core.sln") }
                dir("tvmcore"){ bat ("nuget restore") }
                dir("cdntokenizers"){ bat ("nuget restore") }
                dir("ws_ingest"){ bat ("nuget restore") }
            }        
        }
        stage("Clean"){
            steps{
                sh(label:"clean bin and obj folders", script:"find . -iname 'bin' -o -iname 'obj' | xargs rm -rf")
                sh(label:"clean published folder", script:"rm -rf ./published")
            }
        }
        stage("Build"){
            steps{
                dir("ws_ingest"){
                    bat (label:"Run MSBuild", script:"\"${MSBUILD}\" Ingest\\Ingest.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/"
                        )
                }
            }        
        }

        stage("Zip and Publish"){
            steps{
                dir("published"){  
                    bat (label:"Zip Artifacts", script:"7z.exe a -r ws-ingest-windows-${BRANCH_NAME}.zip *")
                    sh (label:"upload to s3", script:"aws s3 cp ws-ingest-windows-${BRANCH_NAME}.zip s3://${S3_BUILD_BUCKET_NAME}/mediahub/${BRANCH_NAME}/build/ws-ingest-windows-${BRANCH_NAME}.zip")
                }
            }        
        }
    }
}