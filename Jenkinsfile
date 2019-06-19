
@Library('kaltura')_

pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
				withCredentials([usernamePassword(credentialsId: 'bitbucket-token', secretVariable: '$BITBUCKET_TOKEN')]) {
					script {
						docker.build("core:$BRANCH_NAME", "-f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$BITBUCKET_TOKEN .")
					}
				}
            }
        }
    }
}