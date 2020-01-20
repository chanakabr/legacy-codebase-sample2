pipeline {
    agent {
        label 'Jenkins-Windows-2019'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'5'))
    }
    parameters {
        string(name: 'branch', defaultValue: 'master', description: 'Main branch (oldest version is Vision)')
    }
    stages {
        stage("Checkout and restore Core Source"){
            steps{
                dir('Core'){ git(url: 'git@github.com:kaltura/Core.git', branch: "${params.branch}") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("Core"){
                    bat "sh DllVersioning.Core.sh ." 
                }
            }        
        }
        stage("Package Nuget Locally"){
            steps{
                echo "Cleanning Nugets dir before packaging new nugets"
                dir("nugets"){ deleteDir() }

                bat "dotnet pack Core/ConfigurationManager/ConfigurationManager.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/TCMClient/TCMClient.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/StaticHttpContextForNetCore/StaticHttpContextForNetCore.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/KLogMonitor/KLogMonitor.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/CouchBaseExtensions/CouchBaseExtensions.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/CouchbaseManager/CouchbaseManager.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/ODBCWrapper/ODBCWrapper.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/CachingManager/CachingManager.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack Core/CachingProvider/CachingProvider.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/ApiObjects/ApiObjects.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/EventBus.Abstraction/EventBus.Abstraction.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/EventManager/EventManager.csproj -o ${WORKSPACE}/nugets/"
				bat "dotnet pack Core/QueueWrapper/QueueWrapper.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/RabbitQueueWrapper/RabbitQueueWrapper.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/LogReloader/LogReloader.csproj -o ${WORKSPACE}/nugets/"
				bat "dotnet pack Core/KSWrapper/KSWrapper.csproj -o ${WORKSPACE}/nugets/"
            }        
        }
        stage("Publish Nugets"){
            steps { 
                dir("nugets"){ 
                    bat "nuget push ConfigurationManager*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push TCMClient*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push StaticHttpContextForNetCore*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push KLogMonitor*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push CouchBaseExtensions*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push CouchbaseManager*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push ODBCWrapper*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push CachingManager*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push CachingProvider*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push ApiObjects*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push EventBus.Abstraction*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push EventManager*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
					bat "nuget push QueueWrapper*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push RabbitQueueWrapper*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                    bat "nuget push LogReloader*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0"  
                    bat "nuget push KSWrapper*.nupkg -Source http://172.31.36.255:8090/nuget || exit 0" 
                }
            }
        }
       
    }
}