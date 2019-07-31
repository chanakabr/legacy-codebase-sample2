pipeline {
    agent none 
    stages {
        stage('Build and push Phoenix dockers'){
            agent { label 'Ubuntu' } 
            environment{ DOCKER_BUILD_TAG = UUID.randomUUID().toString() }
            steps {
                sh (label: "Docker build dev-phoenix:$DOCKER_BUILD_TAG", 
                    script: "docker build -t dev-phoenix:${DOCKER_BUILD_TAG} --build-arg CORE_BUILD_TAG=${BRANCH_NAME} ."
                )
                script {
                    docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                        docker.image("dev-phoenix:$DOCKER_BUILD_TAG").push("$BRANCH_NAME")
                    }
                }
            }
        }
    }
}