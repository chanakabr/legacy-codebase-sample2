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
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvpapi.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvplibs') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvplibs.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvincicommon') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvincicommon.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("tvpapi") { 
                    bat "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }
                dir("tvplibs") { 
                    bat "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("nuget restore") }
                dir("tvpapi"){ bat ("nuget restore") }
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
                
                dir("tvpapi"){
                    bat (label:"Run MSBuild" , script:"\"${MSBUILD}\" WS_TVPApi\\website.publishproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/"
                    )
                }
            }        
        }

        stage("Zip and Publish"){
            environment {
                    RELEASE_FULL_VERSION = sh(label:"Extract Full Verion Tag", script: 'cd tvpapi && ../Core/get-version-tag.sh', , returnStdout: true).trim()
                    RELEASE_MAIN_VERSION = sh(label:"Extract Main Version Tag", script: "echo $version | sed -e 's/\\.[0-9]*\$//g'", , returnStdout: true).trim();
            }
            steps{
                dir("published"){  
                    bat (label:"Zip Artifacts", script:"7z.exe a -r tvpapi-windows-${BRANCH_NAME}.zip *")
                    withAWS(region:"${S3_BUILD_BUCKET_REGION}") {
                        s3Upload(file:"tvpapi-windows-${BRANCH_NAME}.zip", bucket:"${S3_BUILD_BUCKET_NAME}", path:"mediahub/${BRANCH_NAME}/build/tvpapi-windows-${BRANCH_NAME}.zip")
                    }
                }
            }        
        }
    }
}