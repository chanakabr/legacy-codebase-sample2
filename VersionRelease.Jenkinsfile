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
        booleanParam(name: 'generate_doc_and_clients', defaultValue: false, description: 'Generate Clients and Docs?')

    }
    environment {
        MSBUILD = tool name: 'V4.6.1', type: 'hudson.plugins.msbuild.MsBuildInstallation'
        NUGET = 'c:\\nuget.exe'
    }
    stages {
        stage("Checkout"){
            steps{
                dir('core'){ git(url: 'git@github.com:kaltura/Core.git', branch: "${params.branch}") }
                dir('remotetasks') { git(url: 'git@bitbucket.org:tvinci_dev/remotetasks.git', branch: "${params.branch}") }
                dir('tvmapps') { git(url: 'git@bitbucket.org:tvinci_dev/tvmapps.git', branch: "${params.branch}") }
                dir('tvpapi_rest') { git(url: 'git@bitbucket.org:tvinci_dev/tvpapi_rest.git', branch: "${params.branch}") }
                dir('ws_ingest') { git(url: 'git@bitbucket.org:tvinci_dev/ws_ingest.git', branch: "${params.branch}") }
                
                dir('tvpapi') { git(url: 'git@bitbucket.org:tvinci_dev/tvpapi.git', branch: "${params.branch}") }
                dir('tvplibs') { git(url: 'git@bitbucket.org:tvinci_dev/tvplibs.git', branch: "${params.branch}") }
                dir('tvincicommon') { git(url: 'git@bitbucket.org:tvinci_dev/tvincicommon.git', branch: "${params.branch}") }
                
                dir('configuration'){ git(url: 'git@bitbucket.org:tvinci_dev/configuration.git', branch: "master") }
                dir('clients-generator'){ git(url: 'git@github.com:kaltura/clients-generator.git') }

                dir('celery_tasks'){ git(url: 'git@bitbucket.org:tvinci_dev/celery_tasks.git', branch: "${params.branch}") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("remotetasks") { bat "sh ../core/DllVersioning.Core.sh ."}
                dir("tvpapi_rest") { bat "sh ../core/DllVersioning.Core.sh ."}
                dir("ws_ingest") { bat "sh ../core/DllVersioning.Core.sh ."}
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("${NUGET} restore") }
                dir("remotetasks"){ bat ("${NUGET} restore") }
                dir("tvmapps"){ bat ("${NUGET} restore") }
                dir("tvpapi_rest"){ bat ("${NUGET} restore") }
                dir("ws_ingest"){ bat ("${NUGET} restore") }
            }        
        }
        stage("Publish Localy"){
            steps{
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
            }        
        }
        stage("Patch Config Files"){
            steps{
                dir("configuration") {bat "xcopy PP_${params.config_version_folder_name}\\RemoteTasks\\*.*  ${WORKSPACE}\\published\\tvpapi_rest\\ /K /D /H /Y"}
            }
        }
    }
}