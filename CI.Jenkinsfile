pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters {
        booleanParam(name: 'build_phoenix', defaultValue: true, description: 'Build Phoenix?')
        booleanParam(name: 'build_remote_tasks', defaultValue: true, description: 'Build Remote Tasks?')
        booleanParam(name: 'build_ws_ingest', defaultValue: true, description: 'Build WSIngest?')
        booleanParam(name: 'build_tvpapi', defaultValue: true, description: 'Build TvpAPI?')
        booleanParam(name: 'build_celery_tasks', defaultValue: true, description: 'Build Celery Tasks?')
        booleanParam(name: 'build_clientlibs', defaultValue: true, description: 'Build Clientlibs?')
    }
    environment {
        WORKSPACE = sh(script: 'pwd', , returnStdout: true).trim()
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
    }
    stages {
        stage("Checkout"){
            steps{
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('remotetasks') { git(url: 'https://bitbucket.org/tvinci_dev/remotetasks.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvmapps') { git(url: 'https://bitbucket.org/tvinci_dev/tvmapps.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvpapi_rest') { git(url: 'https://bitbucket.org/tvinci_dev/tvpapi_rest.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('ws_ingest') { git(url: 'https://bitbucket.org/tvinci_dev/ws_ingest.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                
                dir('tvpapi') { git(url: 'https://bitbucket.org/tvinci_dev/tvpapi.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvplibs') { git(url: 'https://bitbucket.org/tvinci_dev//tvplibs.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                dir('tvincicommon') { git(url: 'https://bitbucket.org/tvinci_dev/tvincicommon.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
                
                dir('clients-generator'){ git(url: 'https://github.com/kaltura/clients-generator.git', credentialsId: "github-ott-ci-cd") }

                dir('celery_tasks'){ git(url: 'https://bitbucket.org/tvinci_dev/celery_tasks.git', branch: "${BRANCH_NAME}", credentialsId: "bitbucket-arthur") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ sh "sh DllVersioning.Core.sh ." }
                dir("remotetasks") { sh "sh ../core/DllVersioning.Core.sh ."}
                
                dir("tvpapi_rest") { 
                    sh "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }

                dir("ws_ingest") { sh "sh ../core/DllVersioning.Core.sh ."}
                dir("tvpapi") { powershell "../Core/DllVersioning.ps1 ."}
                dir("tvmapps") { powershell "../Core/DllVersioning.ps1 ."}
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ sh ("nuget restore") }
                dir("remotetasks"){ sh ("nuget restore") }
                dir("tvmapps"){ sh ("nuget restore") }
                dir("tvpapi_rest"){ sh ("nuget restore") }
                dir("ws_ingest"){ sh ("nuget restore") }
                dir("tvpapi"){ sh ("nuget restore") }
            }        
        }
        stage("Build"){
            steps{
                dir("published") { deleteDir() }
                dir("tvpapi_rest"){
                    sh ("\"${MSBUILD}\" Phoenix.Legacy\\Phoenix.Legacy.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/kaltura_ott_api/"
                    )

                    sh ("\"${MSBUILD}\" ConfigurationValidator\\ConfigurationValidator.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:OutputPath=\"${WORKSPACE}/published/configuration_validator/"
                    )

                    sh ("\"${MSBUILD}\" PermissionsExport\\PermissionsDeployment.csproj /m:4"
                        + " /p:Configuration=Release"
                        + " /p:DeleteExistingFiles=True"
                        + " /p:OutputPath=\"${WORKSPACE}/published/permissions/"
                    )
                }

                dir("remotetasks"){
                    sh ("\"${MSBUILD}\" RemoteTasksService\\RemoteTasksService.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/remotetasks/"
                    )
                }

                dir("tvmapps"){
                    sh ("\"${MSBUILD}\" \"Web Sites\\TVM\\TVM.csproj\" /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/tvm/"
                    )
                }

                dir("ws_ingest"){
                    sh ("\"${MSBUILD}\" Ingest\\Ingest.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/ws_ingest/"
                        )
                }

                dir("tvpapi"){
                    sh ("\"${MSBUILD}\" TVPProAPIs.sln /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${WORKSPACE}/published/tvpapi/"
                        )
                }

                dir("celery_tasks"){
                    sh "cp -rf tasks ${WORKSPACE}/published/tasks/"
                }
            }        
        }
        stage("Generate KalturaClient.xml"){
            steps { 
                dir("tvpapi_rest/Generator"){
                    sh ("\"${MSBUILD}\" /p:Configuration=Release /m:4")
                    dir("bin/Release/netcoreapp2.0"){
                        sh ("dotnet Generator.dll")
                        sh ("mkdir ${WORKSPACE}/published/kaltura_ott_api/clientlibs/ || true")
                        sh ("cp -rf KalturaClient.xml ${WORKSPACE}/published/kaltura_ott_api/clientlibs/")
                    }
                }
            }
        }
        stage("Generate Kaltura Clients and Docs"){
            when { expression { return params.build_clientlibs == true; } }
            steps { 
                dir("clients-generator"){
                    sh ("php exec.php --dont-gzip -x${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\KalturaClient.xml "
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
                    bat "7z.exe a -r ${params.branch}_${release_name}${release_number}.zip *"
                    bat "xcopy ${params.branch}_${release_name}${release_number}.zip c:\\version_release\\mediahub\\${release_name}${release_number}\\ /E /O /X /K /D /H /Y"
                }
            }        
        }
    }
}