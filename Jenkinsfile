pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
				withCredentials([usernamePassword(credentialsId: 'bitbucket', usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD')]) {
					script {
						docker.build("core:$BUILD_NUMBER", "-f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$PASSWORD .")
					}
				}
            }
        }
        stage('Push') {
            steps {
				script {
					docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
						docker.image("core:$BUILD_NUMBER").push("netcore-$BRANCH_NAME")
					}
				}
            }
        }
    }
}