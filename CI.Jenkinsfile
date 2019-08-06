pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    // parameters {
    // }
    environment {
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
    }
    stages {
        stage("Checkout"){
            steps{
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('remotetasks') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/remotetasks.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvmapps') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvmapps.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvpapi_rest') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvpapi_rest.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('ws_ingest') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/ws_ingest.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                
                dir('tvpapi') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvpapi.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvplibs') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvplibs.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvincicommon') { git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/tvincicommon.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                
                dir('clients-generator'){ git(url: 'https://github.com/kaltura/clients-generator.git', credentialsId: "github-ott-ci-cd") }

                dir('celery_tasks'){ git(url: 'https://arthurvaverko@bitbucket.org/tvinci_dev/celery_tasks.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("remotetasks") { bat "sh ../core/DllVersioning.Core.sh ."}
                
                dir("tvpapi_rest") { 
                    bat "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }

                dir("ws_ingest") { bat "sh ../core/DllVersioning.Core.sh ."}
                dir("tvpapi") { powershell "../Core/DllVersioning.ps1 ."}
                dir("tvmapps") { powershell "../Core/DllVersioning.ps1 ."}
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("nuget restore") }
                dir("remotetasks"){ bat ("nuget restore") }
                dir("tvmapps"){ bat ("nuget restore") }
                dir("tvpapi_rest"){ bat ("nuget restore") }
                dir("ws_ingest"){ bat ("nuget restore") }
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
                
                dir("tvpapi_rest"){
                    bat ("\"${MSBUILD}\" Phoenix.Legacy\\Phoenix.Legacy.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/kaltura_ott_api/"
                    )


                    bat ("\"${MSBUILD}\" ConfigurationValidator\\ConfigurationValidator.csproj -m:4 -nr:False -t:Restore,Build"
                            + " -p:Configuration=Release"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:OutDir=\"${WORKSPACE}/published/configuration_validator/"
                    )

                    bat ("\"${MSBUILD}\" PermissionsExport\\PermissionsDeployment.csproj -m:4 -nr:False -t:Restore,Build"
                            + " -p:Configuration=Release"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:OutDir=\"${WORKSPACE}/published/permissions/"
                    )
                }

                dir("remotetasks"){
                    bat ("\"${MSBUILD}\" RemoteTasksService\\RemoteTasksService.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/remotetasks/"
                    )
                }

                dir("tvmapps"){
                    bat ("\"${MSBUILD}\" \"Web Sites\\TVM\\TVM.csproj\" -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/tvm/"
                    )
                }

                dir("ws_ingest"){
                    bat ("\"${MSBUILD}\" Ingest\\Ingest.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/ws_ingest/"
                        )
                }

                dir("tvpapi"){
                    bat ("\"${MSBUILD}\" WS_TVPApi\\website.publishproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/tvpapi/"
                        )
                }


                dir("celery_tasks"){
                    bat "xcopy tasks ${WORKSPACE}\\published\\tasks\\ /E /O /X /K /D /H /Y"
                }
            }        
        }

        stage("Generate KalturaClient.xml"){
            steps { 
                dir("tvpapi_rest/Generator"){
                    bat ("\"${MSBUILD}\" -p:Configuration=Release -m:4 -nr:False -t:Restore,Build")
                    dir("bin/Release/netcoreapp2.0"){
                        bat ("dotnet Generator.dll")
                        bat ("xcopy KalturaClient.xml ${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\")
                    }
                }
            }
        }

        stage("Generate Kaltura Clients and Docs"){
            steps { 
                dir("clients-generator"){
                    bat ("php exec.php --dont-gzip -x${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\KalturaClient.xml "
                        +"-tott ottTestme,testmeDoc,php5,php53,php5Zend,php4,csharp,ruby,java,android,python,objc,cli,node,ajax " 
                        +"${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\"
                    )
                }
            }
        }

        stage("Zip and Publish"){
            environment {
                    RELEASE_FULL_VERSION = sh(script: 'cd tvpapi_rest && ../Core/get-version-tag.sh', , returnStdout: true).trim()
                    RELEASE_MAIN_VERSION = sh(script: "echo $version | sed -e 's/\\.[0-9]*\$//g'", , returnStdout: true).trim();
            }
            steps{
                dir("published"){  
                    bat "7z.exe a -r ${BRANCH_NAME}.zip *"
                    withAWS(region:'eu-west-1') {
                        s3Upload(file:"${BRANCH_NAME}.zip", bucket:'ott-be-builds', path:"mediahub/${BRANCH_NAME}/build/${BRANCH_NAME}.zip")
                    }
                    echo "upload to S3 here"
                }
            }        
        }
    }
}