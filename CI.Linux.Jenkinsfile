def GIT_COMMIT=""
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
        REPOSITORY_NAME="${BRANCH_NAME.toLowerCase()}/tvpapi"
        ECR_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${REPOSITORY_NAME}"
        ECR_CORE_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${BRANCH_NAME.toLowerCase()}/core"
    }
    stages {
        stage('Checkout'){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi') { git(url: 'https://github.com/kaltura/tvpapi.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }

                script{
                    dir("tvpapi"){ GIT_COMMIT = sh(returnStdout: true, script: 'git rev-parse HEAD').trim() }
                }
            }
        }
        stage('Build Docker'){
            environment{
                FULL_VERSION = sh(script: 'cd tvpapi && ../core/get-version-tag.sh', , returnStdout: true).trim()
            }
            steps{
                dir("tvpapi"){
                    sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")

                    sh(label: "Validate we have latest core docker image", script: "docker pull ${ECR_CORE_REPOSITORY}:build")
                    sh(
                        label: "Docker build core:${BRANCH_NAME.toLowerCase()}", 
                        script: "docker build "+
                        "-t ${ECR_REPOSITORY}:build  "+
                        "-t ${ECR_REPOSITORY}:${GIT_COMMIT} "+
                        "--build-arg BRANCH=${BRANCH_NAME} "+
                        "--build-arg CORE_IMAGE=${ECR_CORE_REPOSITORY} "+
                        "--build-arg CORE_BUILD_TAG=build "+
                        "--label 'version=${FULL_VERSION}' "+
                        "--label 'commit=${GIT_COMMIT}' "+
                        "--label 'build=${env.BUILD_NEMBER}' ."
                    )
                }
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
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:${GIT_COMMIT}")
            }
        }

        stage('Trigger Release Candidate'){
            build job: 'OTT-BE-Create-Release-Candidate', parameters: [
                                                string(name: 'BRANCH_NAME', value: "${BRANCH_NAME}"),
                                                ], wait: false

        }
    }
    post {
        stage('Report To Dynamo'){
            configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
            def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} build ${env.BUILD_NUMBER} ${env.JOB_NAME} build ${currentBuild.currentResult} ", returnStdout: true)
            echo "${report}"
        }
    }
}