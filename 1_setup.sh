apt-get update
add-apt-repository ppa:dotnet/backports
apt-get install -y unzip dotnet-sdk-9.0
snap install odcey
cmake .
conda create -p venv python==3.12.3
conda activate venv/
#source ./venv/bin/activate
pip install -r requirements.txt
