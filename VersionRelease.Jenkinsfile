pipeline {
    agent {
        label 'Jenkins-Windows-2019'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters {
        string(name: 'branch', defaultValue: 'master', description: 'Branch To Release')
        choice(name: 'release_name', choices: ['SP', 'FP', 'HD'], description: 'Release Name')
        string(name: 'release_number', defaultValue: '0', description: 'Release_Number')
        booleanParam(name: 'generate_doc_and_clients', defaultValue: true, description: 'Generate Clients and Docs?')

    }
    environment {
        MSBUILD = tool name: 'V4.6.1', type: 'hudson.plugins.msbuild.MsBuildInstallation'
        NUGET = 'c:\\nuget.exe'
        ROOT_PUBLISH_DIR = "c:\\version_release\\mediahub\\${params.branch}"
        TEMP_PUBLISH_DIR = "${ROOT_PUBLISH_DIR}\\${BUILD_TIMESTAMP}"
        FINAL_PUBLISH_DIR = "${ROOT_PUBLISH_DIR}\\${release_name}${release_number}"
    }
    stages {
        stage("Clean"){
            steps{
                deleteDir()
            }
        }
        stage("Checkout"){
            steps{
                dir('core'){ git(url: 'git@github.com:kaltura/Core.git', branch: "${params.branch}") }
                dir('remotetasks') { git(url: 'git@github.com:kaltura/RemoteTasks.git', branch: "${params.branch}") }
                dir('tvmapps') { git(url: 'git@github.com:kaltura/tvmapps.git', branch: "${params.branch}") }
                dir('tvpapi_rest') { git(url: 'git@github.com:kaltura/Phoenix.git', branch: "${params.branch}") }
                dir('ws_ingest') { git(url: 'git@github.com:kaltura/WS_Ingest.git', branch: "${params.branch}") }
                
                dir('tvpapi') { git(url: 'git@github.com:kaltura/tvpapi.git', branch: "${params.branch}") }
                dir('tvplibs') { git(url: 'git@github.com:kaltura/tvplibs.git', branch: "${params.branch}") }
                dir('tvincicommon') { git(url: 'git@github.com:kaltura/TvinciCommon.git', branch: "${params.branch}") }
                
                dir('clients-generator'){ git(url: 'git@github.com:kaltura/clients-generator.git') }

                dir('celery_tasks'){ git(url: 'git@github.com:kaltura/ott-celery-tasks.git', branch: "${params.branch}") }
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
                dir("tvpapi_rest"){ 
                    bat ("${NUGET} restore") 
                    bat ("dotnet restore") 
                }
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
                            + " /p:publishUrl=${TEMP_PUBLISH_DIR}\\kaltura_ott_api\\"
                    )

                    bat ("\"${MSBUILD}\" ConfigurationValidator\\ConfigurationValidator.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:OutputPath=${TEMP_PUBLISH_DIR}\\configuration_validator\\"
                    )

                    bat ("\"${MSBUILD}\" PermissionsExport\\PermissionsDeployment.csproj /m:4"
                        + " /p:Configuration=Release"
                        + " /p:DeleteExistingFiles=True"
                        + " /p:OutputPath=${TEMP_PUBLISH_DIR}\\permissions\\"
                    )

                    dir("${TEMP_PUBLISH_DIR}/permissions"){
                        bat("PermissionsDeployment.exe e=permissions.xml")
                    }
                }

                dir("remotetasks"){
                    bat ("\"${MSBUILD}\" RemoteTasksService\\RemoteTasksService.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=${TEMP_PUBLISH_DIR}\\remotetasks\\"
                    )
                }

                dir("tvmapps"){
                    bat ("\"${MSBUILD}\" \"Web Sites\\TVM\\TVM.csproj\" /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=${TEMP_PUBLISH_DIR}\\tvm\\"
                    )
                }

                dir("ws_ingest"){
                    bat ("\"${MSBUILD}\" Ingest\\Ingest.csproj /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=${TEMP_PUBLISH_DIR}\\ws_ingest\\"
                        )
                }

                dir("tvpapi"){
                    bat ("\"${MSBUILD}\" \"TVPApi.Legacy\\TVPApi.Legacy.csproj\" /m:4"
                            + " /p:Configuration=Release"
                            + " /p:DeployDefaultTarget=WebPublish"
                            + " /p:WebPublishMethod=FileSystem"
                            + " /p:DeleteExistingFiles=True"
                            + " /p:DeployOnBuild=True"
                            + " /p:publishUrl=\"${TEMP_PUBLISH_DIR}\\TVPAPI\\"
                        )
                }

                dir("celery_tasks"){
                    bat "xcopy tasks ${TEMP_PUBLISH_DIR}\\tasks\\ /E /O /X /K /D /H /Y"
                }
            }        
        }

        stage("Generate KalturaClient.xml"){
            steps { 
                dir("tvpapi_rest/Generator"){
                    bat ("dotnet build -c Release")
                    dir("bin/Release/netcoreapp2.0"){
                        bat ("dotnet Generator.dll")
                        bat ("xcopy KalturaClient.xml ${TEMP_PUBLISH_DIR}\\kaltura_ott_api\\clientlibs\\")
                    }
                }
            }
        }

        stage("Generate Kaltura Clients and Docs"){
            when { expression { return params.generate_doc_and_clients == true; } }
            steps { 
                dir("clients-generator"){
                    bat ("php exec.php --dont-gzip -x${TEMP_PUBLISH_DIR}\\kaltura_ott_api\\clientlibs\\KalturaClient.xml "
                        +"-tott ottTestme,testmeDoc,php5,php53,php5Zend,php4,csharp,ruby,java,android,python,objc,cli,node,ajax " 
                        +"${TEMP_PUBLISH_DIR}\\kaltura_ott_api\\clientlibs\\"
                    )
                }
            }
        }

        stage("Zip and Publish"){
            steps{
                dir("${ROOT_PUBLISH_DIR}"){  
                    // \\34.252.63.117\version_release\mediahub\5_2_4\SP0\5_2_4_SP0.zip
                    bat "7z.exe a -r ${params.branch}_${release_name}${release_number}.zip ${BUILD_TIMESTAMP}"
                    bat "if not exist ${FINAL_PUBLISH_DIR} mkdir ${FINAL_PUBLISH_DIR}"
                    bat "move /Y ${params.branch}_${release_name}${release_number}.zip ${FINAL_PUBLISH_DIR}\\"
                }
            }        
        }
    }
    post {
        always {
            emailext (
                subject: " [${currentBuild.currentResult}] Job: [${env.JOB_NAME}]",
                to: "ott.rnd.core@kaltura.com",
                mimeType : "text/html",
                body: "[${currentBuild.currentResult}] <a href='${env.BUILD_URL}'>Job: ${env.JOB_NAME} Build#: ${env.BUILD_NUMBER} </a><br/>"+
                "Path:\\\\34.252.63.117\\version_release\\mediahub\\${release_name}${release_number}\\${params.branch}_${release_name}${release_number}.zip <br/>"
            )

        }
    }
}