
@Library('kaltura')_

pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
                script {
                    docker.build("core:$BRANCH_NAME", "-t kaltura/core:$version -f NetCore.Dockerfile . --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$BITBUCKET_TOKEN .")
                }
            }
        }
    }
}