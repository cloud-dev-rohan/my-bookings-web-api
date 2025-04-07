# MyBookingsWebApi

## Overview
MyBookingsWebApi is a scalable and fault-tolerant web API built using **.NET 8.0**, **PostgreSQL**, and **RabbitMQ**. It allows users to upload CSV files, manage inventory bookings, and handle cancellations.
## Architecture
![architecture_webapi drawio](https://github.com/user-attachments/assets/b5561e13-5786-447a-a9f9-5c3fb6272bc3)

## Tech Stack
- **Backend:** .NET 8.0 Web API
- **Database:** PostgreSQL
- **Message Broker:** RabbitMQ, MassTransit
- **Containerization:** Docker, Docker Compose
- **CI/CD:** Azure Pipelines
- **Deployment:** Azure App Service, Azure Kubernetes Service (AKS)

## Prerequisites
Ensure you have the following installed:
- .NET 8.0 SDK
- Docker & Docker Compose
- Azure CLI (for Azure deployment)
- kubectl (for Kubernetes deployment)

---

## Running Locally with Docker Compose
1. Clone the repository:
   ```sh
   git clone https://github.com/cloud-dev-rohan/my-bookings-web-api.git
   change the directory to MyBookingsWebApi
   ```
2. Build and run the services:
   ```sh
   docker-compose up --build
   ```
3. The API should now be available at:
   ```sh
   http://localhost:5000
   ```
4. RabbitMQ Management UI (Optional):
   ```sh
   http://localhost:15672 (user: guest, password: guest)
   ```

---

## Deploying to Azure App Service
### 1. Build and Push Docker Image to Azure Container Registry (ACR)
```sh
az acr create --resource-group MyResourceGroup --name MyContainerRegistry --sku Basic
az acr login --name MyContainerRegistry
az acr build --image mybookingswebapi:latest --registry MyContainerRegistry --file Dockerfile .
```

### 2. Create and Deploy to Azure App Service
```sh
az webapp create --resource-group MyResourceGroup --plan MyAppServicePlan --name MyBookingsWebApi --deployment-container-image-name MyContainerRegistry.azurecr.io/mybookingswebapi:latest
```

---

## Deploying to Azure Kubernetes Service (AKS)
### 1. Create AKS Cluster
```sh
az aks create --resource-group MyResourceGroup --name MyAKSCluster --node-count 2 --enable-addons monitoring --generate-ssh-keys
```

### 2. Connect to AKS and Deploy
```sh
az aks get-credentials --resource-group MyResourceGroup --name MyAKSCluster
kubectl apply -f k8s-deployment.yaml
```

---

## Kubernetes YAML Files
### k8s-deployment.yaml
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: webapi
spec:
  replicas: 2
  selector:
    matchLabels:
      app: webapi
  template:
    metadata:
      labels:
        app: webapi
    spec:
      containers:
        - name: webapi
          image: youracr.azurecr.io/webapi:latest
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: Development
            - name: ConnectionStrings__DefaultConnection
              value: "Host=postgres-service;Port=5432;Database=mydb;Username=myuser;Password=mypassword"
            - name: RABBITMQ_HOST
              value: "rabbitmq://rabbitmq"
            - name: RABBITMQ_USER
              value: guest
            - name: RABBITMQ_PASS
              value: guest
      restartPolicy: Always
---
apiVersion: v1
kind: Service
metadata:
  name: webapi-service
spec:
  selector:
    app: webapi
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080
  type: LoadBalancer
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres-db
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres-db
  template:
    metadata:
      labels:
        app: postgres-db
    spec:
      containers:
        - name: postgres-db
          image: postgres:15
          ports:
            - containerPort: 5432
          env:
            - name: POSTGRES_DB
              value: mydb
            - name: POSTGRES_USER
              value: myuser
            - name: POSTGRES_PASSWORD
              value: mypassword
          volumeMounts:
            - name: postgres-storage
              mountPath: /var/lib/postgresql/data
      volumes:
        - name: postgres-storage
          emptyDir: {}
---
apiVersion: v1
kind: Service
metadata:
  name: postgres-service
spec:
  selector:
    app: postgres-db
  ports:
    - protocol: TCP
      port: 5432
      targetPort: 5432
  type: ClusterIP
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rabbitmq
spec:
  replicas: 1
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
        - name: rabbitmq
          image: rabbitmq:3-management
          ports:
            - containerPort: 5672
            - containerPort: 15672
          env:
            - name: RABBITMQ_DEFAULT_USER
              value: guest
            - name: RABBITMQ_DEFAULT_PASS
              value: guest
---
apiVersion: v1
kind: Service
metadata:
  name: rabbitmq-service
spec:
  selector:
    app: rabbitmq
  ports:
    - protocol: TCP
      port: 5672
      targetPort: 5672
    - protocol: TCP
      port: 15672
      targetPort: 15672
  type: ClusterIP
```


---

## Azure Pipelines YAML for CI/CD
```yaml
trigger:
  branches:
    include:
      - master

pool:
  vmImage: 'ubuntu-latest'

stages:
  - stage: Build
    jobs:
      - job: Build
        steps:
          - task: Docker@2
            inputs:
              containerRegistry: 'myACR'
              repository: 'webapi'
              command: 'buildAndPush'
              Dockerfile: '**/Dockerfile'
              tags: 'latest'

  - stage: Deploy
    dependsOn: Build
    jobs:
      - job: Deploy
        steps:
          - task: Kubernetes@1
            inputs:
              connectionType: 'Azure Resource Manager'
              azureSubscription: 'your-azure-subscription'
              azureResourceGroup: 'myResourceGroup'
              kubernetesCluster: 'myAKSCluster'
              namespace: 'default'
              command: 'apply'
              useConfigurationFile: true
              configuration: 'k8-deployment.yml'
```

---

## Verifying the Deployment
Once deployed, you can access the API:
- **Azure App Service:** `https://mybookingswebapi.azurewebsites.net`

---

