pipeline {
    agent {
        label 'Linux'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters{
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch Name')
    }
    environment{
        AWS_REGION="us-west-2"
        REPOSITORY_NAME="${BRANCH_NAME}/core"
        ECR_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${REPOSITORY_NAME}"
    }
    stages {
        stage('Checkout'){
            steps{
                git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd")
            }
        }
        stage('Build Docker'){
            environment{
                FULL_VERSION = sh(script: './get-version-tag.sh', , returnStdout: true).trim()
            }
            steps{
                sh(
                    label: "Docker build core:${BRANCH_NAME}", 
                    script: "docker build -t ${ECR_REPOSITORY}:build  "+
                    "-f NetCore.Dockerfile "+
                    "--build-arg BRANCH=${BRANCH_NAME} "+
                    "--label 'version=${FULL_VERSION}' "+
                    "--label 'commit=${env.GIT_COMMIT}' "+
                    "--label 'build=${env.BUILD_NEMBER}' ."
                )
            }
        }
        stage('Push to ECR'){
            steps{
                sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")
                sh(
                    label: "Verify ECR Repository Exist", 
                    script: "aws ecr describe-repositories --repository-names ${REPOSITORY_NAME} --region ${AWS_REGION} || "+
                            "aws ecr create-repository --repository-name ${REPOSITORY_NAME} --region ${AWS_REGION}"
                )
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:build")
            }
        }
        stage("Build Phoenix"){
            steps{
                build (job: "OTT-BE-Phoenix-Linux", parameters: [
                    [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                ]) 
            }
        }
    }
}