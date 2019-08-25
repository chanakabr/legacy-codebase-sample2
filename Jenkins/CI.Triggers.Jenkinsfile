node {
    def BRANCH_NAME = ""
    def JOBS_TO_RUN = []
    stage('Extract Trigger Params'){
        BRANCH_NAME = REF.replaceAll("refs/heads/", "") 
        JOBS_TO_RUN = getJobName(REPOSITORY_NAME)
        echo "extracted branch from ref:[${REF}] ==> [${BRANCH_NAME}] "
        echo "extracted job name from repository_name[${REPOSITORY_NAME}] ==> [${JOBS_TO_RUN}] "
        if (BRANCH_NAME?.isAllWhitespace() || JOBS_TO_RUN.isEmpty()){
            error("Could not identify job or/and branch to trigger...");
        }
    }
    stage('Trigger Relevant Job'){
        for(JOB_TO_RUN in JOBS_TO_RUN){
            build (
                job: "${JOB_TO_RUN}", 
                wait: false,
                parameters: [
                    [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                ]
            )
        }
    }
}

def getJobName(repoName) {
    switch (repoName) {
        case 'Core':
            return ['OTT-BE-Core-Windows', 'OTT-BE-Core-Linux']
        case 'Phoenix':
            return ['OTT-BE-Phoenix-Windows', 'OTT-BE-Phoenix-Linux']
        case 'RemoteTasks':
            return ['OTT-BE-Remote-Tasks-Windows']
        case 'tvmapps':
            return ['OTT-BE-TVM']
        case 'tvpapi':
            return ['OTT-BE-Tvpapi-Windows', 'OTT-BE-Tvpapi-Linux']
        case 'ott-celery-tasks':
            return ['OTT-BE-Celery-Tasks']
        case 'WS_Ingest':
            return ['OTT-BE-WS-Ingest-Windows']
        default:
            return []
    }   
}

