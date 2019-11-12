pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters{
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch Name')
    }
    stages {
        stage('Run Parallel Builds') {
            parallel {
                stage('Remote Tasks') {
                    steps{ 
                        build (job: 'OTT-BE-Remote-Tasks-Windows', parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ])
                    }
                }
                
                stage('Phoenix') {
                    steps{ 
                        build (job: "OTT-BE-Phoenix-Windows", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }

                stage('WS-Ingest') {
                    steps{ 
                        build (job: "OTT-BE-WS-Ingest-Windows", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }

                stage('TVM') {
                    steps{ 
                        build (job: "OTT-BE-TVM", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }

                stage('Tvpapi') {
                    steps{ 
                        build (job: "OTT-BE-TVPAPI-Windows", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }
                
            }
                
            
         
        }
        stage('Report to CI DynamoDB'){
            steps{
                    report()
            }
        }
    }
}


def report(){
    configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
    def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} build ${env.BUILD_NUMBER} ${env.JOB_NAME} build ${currentBuild.currentResult}", returnStdout: true)
    echo "${report}"
    // return report
}