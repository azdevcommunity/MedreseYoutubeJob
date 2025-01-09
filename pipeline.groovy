pipeline {
    agent any

    environment {
        GIT_CREDENTIALS = 'github-cred'
        IMAGE_NAME = "medrese-job"
        CONTAINER_NAME_PREFIX = "medrese-backend-container"
        DATABASE_HOST = "31.220.95.127"
        DATABASE_USERNAME = "postgres"
        DATABASE_PASSWORD = "123456789",
        DATABASE_PORT = 5433,
        DATABASE_NAME = "esmdb",
        PORT = "8085"
//         CONNECTION_STRING = "Host=31.220.95.127;Port=5433;Database=esmdb;Username=postgres;Password=123456789"
    }

    stages {
        stage('Clone Repository') {
            steps {
                // Checkout the code from your repository using credentials
                git branch: 'main', url: 'https://github.com/azdevcommunity/MedreseYoutubeJob', credentialsId: "${GIT_CREDENTIALS}"
                script {
                    // Commit hash'i alma
                    env.GIT_COMMIT = sh(script: "git rev-parse --short HEAD", returnStdout: true).trim()
                    // IMAGE_TAG'ı oluştur
                    env.IMAGE_TAG = "${env.BUILD_NUMBER}-${env.GIT_COMMIT}"
                    // Yeni container adını oluştur
                    env.NEW_CONTAINER_NAME = "${CONTAINER_NAME_PREFIX}-${IMAGE_TAG}"
                    env.NEW_IMAGE_NAME = "${IMAGE_NAME}:${IMAGE_TAG}"
                }
            }
        }

      

        stage('Build Docker Image') {
            steps {
                script {
                    // Build the Docker image with a unique tag
                    sh "docker build -t ${IMAGE_NAME}:${IMAGE_TAG} ."
                    sh "docker images"
                }
            }
        }

        stage('Stop Existing Container Temporarily') {
            steps {
                script {
                    // Stop the container currently using the port
                    def containerUsingPort = sh(script: "docker ps --filter 'publish=${PORT}' --format '{{.ID}}'", returnStdout: true).trim()
                    if (containerUsingPort) {
                        echo "Temporarily stopping container using port ${PORT}: ${containerUsingPort}"
                        sh "docker stop ${containerUsingPort} || true"
                    }
                }
            }
        }

        stage('Run New Container') {
            steps {
                script {
                    // Run the new container
                    sh """
                    docker run -d \
                    -e DATABASE_HOST=${DATABASE_HOST} \
                    -e DATABASE_USERNAME=${DATABASE_USERNAME} \
                    -e DATABASE_PASSWORD=${DATABASE_PASSWORD} \
                    -e DATABASE_PORT=${DATABASE_PORT} \
                    -e DATABASE_NAME=${DATABASE_NAME} \
                    -e PORT=${PORT} \
                    -p ${PORT}:${PORT} \
                    -v /var/www/esm/uploads:/app/uploads \
                    --network br10_network \
                    --network-alias medrese-job \
                    --name ${NEW_CONTAINER_NAME} ${IMAGE_NAME}:${IMAGE_TAG}
                    """
                    sh "docker ps -a"
                }
            }
        }

        stage('Health Check and Cleanup') {
            steps {
                script {
                    // Check if the new container is running successfully
                    def newContainer = sh(script: "docker ps -qf 'name=${NEW_CONTAINER_NAME}'", returnStdout: true).trim()
                    if (newContainer) {
                        echo "New container is running successfully: ${newContainer}"
                        // Stop and remove old containers
                        sh "docker ps --filter 'name=^${CONTAINER_NAME_PREFIX}-' --filter 'status=exited' --format '{{.Names}}' | xargs -I {} docker stop {}"
                        sh "docker ps --filter 'name=^${CONTAINER_NAME_PREFIX}-' --filter 'status=exited' --format '{{.Names}}' | xargs -I {} docker rm {}"
                    } else {
                        echo "New container failed to start. Restarting old container."
                        sh "docker ps -a --filter 'name=^${CONTAINER_NAME_PREFIX}-' --filter 'status=exited' --format '{{.Names}}' | head -n 1 | xargs -I {} docker start {}"
                        error "New container failed, old container restarted."
                    }
                }
            }
        }

        stage('Clean Up Old Images') {
            steps {
                script {
                    // Remove old images
                    sh "docker images --filter 'reference=${IMAGE_NAME}*' --format '{{.Repository}}:{{.Tag}}' | grep -v '${NEW_IMAGE_NAME}' | xargs -I {} docker rmi {}"
                }
            }
        }
    }

    post {
        failure {
            script {
                // If the new container fails, clean up
                def failedContainer = sh(script: "docker ps -aqf 'name=${NEW_CONTAINER_NAME}'", returnStdout: true).trim()
                if (failedContainer) {
                    sh "docker stop ${failedContainer} || true"
                    sh "docker rm ${failedContainer} || true"
                    sh "docker rmi ${IMAGE_NAME}:${IMAGE_TAG} || true"
                }
            }
        }
    }
}