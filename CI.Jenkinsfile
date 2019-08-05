pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters {
    }
    environment {
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
        NUGET = 'c:\\nuget.exe'
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
                dir("core"){ bat ("${NUGET} restore") }
                dir("remotetasks"){ bat ("${NUGET} restore") }
                dir("tvmapps"){ bat ("${NUGET} restore") }
                dir("tvpapi_rest"){ bat ("${NUGET} restore") }
                dir("ws_ingest"){ bat ("${NUGET} restore") }
                dir("tvpapi"){ bat ("${NUGET} restore") }
            }        
        }
        stage("Build"){
            steps{
                dir("published") { deleteDir() }
                dir("tvpapi_rest"){
                    bat ("\"${MSBUILD}\" Phoenix.Legacy\\Phoenix.Legacy.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/kaltura_ott_api/"
                    )

                    bat ("\"${MSBUILD}\" ConfigurationValidator\\ConfigurationValidator.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:OutputPath=\"${WORKSPACE}/published/configuration_validator/"
                    )

                    bat ("\"${MSBUILD}\" PermissionsExport\\PermissionsDeployment.csproj /m:4"
                        + " /p:Configuration=Release"
                        + " /p:DeleteExistingFiles=True"
                        + " /p:OutputPath=\"${WORKSPACE}/published/permissions/"
                    )
                }

                dir("remotetasks"){
                    bat ("\"${MSBUILD}\" RemoteTasksService\\RemoteTasksService.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/remotetasks/"
                    )
                }

                dir("tvmapps"){
                    bat ("\"${MSBUILD}\" \"Web Sites\\TVM\\TVM.csproj\" /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/tvm/"
                    )
                }

                dir("ws_ingest"){
                    bat ("\"${MSBUILD}\" Ingest\\Ingest.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/ws_ingest/"
                        )
                }

                dir("tvpapi"){
                    bat ("\"${MSBUILD}\" TVPProAPIs.sln /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/tvpapi/"
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
                    bat ("\"${MSBUILD}\" /p:Configuration=Release /m:4")
                    dir("bin/Release/netcoreapp2.0"){
                        bat ("dotnet Generator.dll")
                        bat ("xcopy KalturaClient.xml ${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\")
                    }
                }
            }
        }

        stage("Generate Kaltura Clients and Docs"){
            when { expression { return params.generate_doc_and_clients == true; } }
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
            steps{
                dir("published"){  
                    // \\34.252.63.117\version_release\mediahub\5_2_4\SP0\5_2_4_SP0.zip
                    //bat "7z.exe a -r ${params.branch}_${release_name}${release_number}.zip *"
                    //bat "xcopy ${params.branch}_${release_name}${release_number}.zip c:\\version_release\\mediahub\\${release_name}${release_number}\\ /O /X /K /D /H /Y"
                    echo "upload to S3 here"
                }
            }        
        }
    }
}