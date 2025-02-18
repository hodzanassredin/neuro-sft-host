wget -O ../../datasets/bb.zip https://informatika-21.ru/rsrc/BlackBox.Shkola.2014b.zip
unzip ../../datasets/bb.zip -d ../../datasets/bb-schola
python3 convert_bb_dir_to_txt_dir.py ../../datasets/bb-schola ../../datasets/bb-schola-txt
python3 fix_enc.py ../../datasets/bb-schola-txt ../../datasets/bb-schola-txt-fixed