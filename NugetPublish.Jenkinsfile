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
                dir('TvmCore'){ git(url: 'git@bitbucket.org:tvinci_dev/tvmcore.git', branch: "${params.branch}") }
                dir('Core'){ git(url: 'git@github.com:kaltura/Core.git', branch: "${params.branch}") }
                dir('CDNTokenizers'){ git(url: 'git@bitbucket.org:tvinci_dev/CDNTokenizers.git', branch: "${params.branch}") }
                dir('tvincicommon'){ git(url: 'git@bitbucket.org:tvinci_dev/tvincicommon.git', branch: "${params.branch}") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("Core"){
                    bat "sh DllVersioning.Core.sh ." 
                    bat "sh DllVersioning.Core.sh ../TvmCore" 
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
                bat "dotnet pack TvmCore/KLogMonitor/KLogMonitor.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack TvmCore/CouchBaseExtensions/CouchBaseExtensions.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack TvmCore/CouchbaseManager/CouchbaseManager.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack TvmCore/ODBCWrapper/ODBCWrapper.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack TvmCore/CachingManager/CachingManager.csproj -o ${WORKSPACE}/nugets/" 
                bat "dotnet pack TvmCore/CachingProvider/CachingProvider.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/ApiObjects/ApiObjects.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/EventBus.Abstraction/EventBus.Abstraction.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/EventManager/EventManager.csproj -o ${WORKSPACE}/nugets/"
				bat "dotnet pack Core/QueueWrapper/QueueWrapper.csproj -o ${WORKSPACE}/nugets/"
                bat "dotnet pack Core/RabbitQueueWrapper/RabbitQueueWrapper.csproj -o ${WORKSPACE}/nugets/"
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
                }
            }
        }
       
    }
}