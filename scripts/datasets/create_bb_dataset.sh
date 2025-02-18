wget -O $1/bb.zip https://informatika-21.ru/rsrc/BlackBox.Shkola.2014b.zip
unzip $1/bb.zip -d $1/bb-schola
python3 convert_bb_dir_to_txt_dir.py $1/bb-schola $1/bb-schola-txt
python3 fix_enc.py $1/bb-schola-txt $1/bb-schola-txt-fixed