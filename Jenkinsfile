
@Library('kaltura')_

pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
                script {
                    def props = readProperties file: 'build.properties'
                    env.version = props['version']
					def appVersion = env.version.replaceAll("[.]","_")
					def tcmApp = "REMOTE_TASKS_v${appVersion}"
					
					docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
	                    docker.build("remote-tasks:$BUILD_NUMBER", "--build-arg VERSION=$version --build-arg CORE_BUILD_TAG=netcore-$BRANCH_NAME --build-arg TCM_APP=$tcmApp .")
					}
                }
            }
        }
        stage('Deploy') {
            steps {
                deploy('remote-tasks', "$version")
            }
        }
    }
}