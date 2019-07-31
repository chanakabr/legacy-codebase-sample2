pipeline {
    agent { label 'Ubuntu' }
    environment{ DOCKER_BUILD_TAG = UUID.randomUUID().toString() }
    stages {
        stage('Build Remote-tasks dockers'){
            steps {
                sh (label: "Docker pull 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BRANCH_NAME}", 
                    script: "docker build -t remote-tasks:${DOCKER_BUILD_TAG} --build-arg CORE_BUILD_TAG=${BRANCH_NAME} ."
                )
                sh (label: "Docker build remote-tasks:$DOCKER_BUILD_TAG", 
                    script: "docker build -t remote-tasks:${DOCKER_BUILD_TAG} --build-arg CORE_BUILD_TAG=${BRANCH_NAME} ."
                )
            }
        }
        stage('Push Phoenix dockers'){
            steps {
                script {
                    docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                        docker.image("remote-tasks:$DOCKER_BUILD_TAG").push("$BRANCH_NAME")
                    }
                }
            }
        }
    }
}