
@Library('kaltura')_

pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
				withCredentials([usernamePassword(credentialsId: 'bitbucket', usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD')]) {
					script {
						docker.build("core:$BRANCH_NAME", "-f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$PASSWORD .")
					}
				}
            }
        }
    }
}