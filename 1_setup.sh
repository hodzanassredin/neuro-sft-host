apt-get update
add-apt-repository ppa:dotnet/backports
apt-get install -y unzip dotnet-sdk-9.0
snap install odcey
cmake .
python3 -m venv ./venv
source ./venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt
