pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    stages {
        stage('Run Parallel Builds') {
            parallel {
                stage('Remote Tasks') {
                    steps{ build (job: "OTT-BE-Remote-Tasks-Windows/${BRANCH_NAME}") }
                }
                stage('Phoenix') {
                    steps{ build (job: "OTT-BE-Phoenix-Windows/${BRANCH_NAME}") }
                }
            }
        }
    }
}