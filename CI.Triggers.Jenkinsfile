node {
    stage('Extract Trigger Params'){
        def BRANCH_NAME = REF.replaceAll("refs/heads/", "") 
        def JOB_TO_RUN = getJobName(REPOSITORY_NAME)
        echo "extracted branch from ref:[${REF}] ==> [${BRANCH_NAME}] "
        echo "extracted job name from repository_name[${REPOSITORY_NAME}] ==> [${JOB_TO_RUN}] "
    }
    stage('Trigger Relevant Job'){
        build (
            job: 'OTT-BE-WS-Ingest-Windows', 
            wait: false,
            parameters: [
                [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
            ]
        )
    }
}

def getJobName(repoName) {
    switch (repoName) {
        case 'Core':
            return 'OTT-BE-Core-Windows'
        case 'RemoteTasks':
            return 'OTT-BE-Remote-Tasks-Windows'
        case 'tvmapps':
            return 'OTT-BE-TVM'
        case 'tvpapi':
            return 'OTT-BE-Tvpapi-Windows'
        case 'ott-celery-tasks':
            return 'OTT-BE-Celery-Tasks'
        case 'WS_Ingest':
            return 'OTT-BE-WS-Ingest-Windows'
    }   
}