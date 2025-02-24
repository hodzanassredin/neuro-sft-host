apt-get update
add-apt-repository ppa:dotnet/backports
apt-get install -y unzip dotnet-sdk-9.0 pre-commit
snap install odcey
cmake .
conda create -p venv python==3.12.3
conda activate venv/
#source ./venv/bin/activate
pip install -r requirements.txt
clearml-init
dotnet dev-certs https --trust
dotnet dev-certs https -ep ./docker/cert/aspnetapp.pfx -p test --trust
openssl pkcs12 -in ./docker/cert/aspnetapp.pfx -nocerts -nodes -out ./docker/cert/aspnetapp.key
openssl pkcs12 -in ./docker/cert/aspnetapp.pfx -out ./docker/cert/aspnetapp.crt -nokeys -clcerts