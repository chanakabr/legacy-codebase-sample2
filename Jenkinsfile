
@Library('kaltura')_

pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
                def props = readProperties file: 'build.properties'
                def version = props['version']
                script {
					def appVersion = version.replaceAll("[.]","_")
					def tcmApp = "REMOTE_TASKS_v${appVersion}"
                    docker.build("remote-tasks:$BUILD_NUMBER", "--build-arg VERSION=$version --build-arg CORE_BUILD_TAG=netcore-$BRANCH_NAME --build-arg TCM_APP=$tcmApp .")
                }
                deploy('remote-tasks', "$version")
            }
        }
    }
}