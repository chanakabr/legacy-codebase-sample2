pipeline {

    agent { 
        label 'Ubuntu'
    }

    stages { 
        stage('Build') {
            steps {
                script {
                    env.DOCKER_BUILD_TAG = UUID.randomUUID().toString()
                    withCredentials([usernamePassword(credentialsId: 'bitbucket', usernameVariable: 'USERNAME', passwordVariable: 'TOKEN')]) {
                        sh label: "Docker build core:$DOCKER_BUILD_TAG", script: "docker build -t core:$DOCKER_BUILD_TAG -f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$TOKEN ."
                    }
                }
            }
        }
        stage('Push') {
            steps {
                script {
                    docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                        docker.image("core:$DOCKER_BUILD_TAG").push("netcore-$BRANCH_NAME")
                    }
                }
            }
        }
    }
}