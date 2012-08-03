#!/bin/sh
echo "This script will reconfigure subversion to work with certs correctly."
echo "Steps outlined by dcrooke and compiled into this script by Kalosaurusrex"
echo "Please see the ubuntuforums.org thread for more information, questions or help."
echo "http://ubuntuforums.org/showthread.php?p=6057983"
echo ""
echo ""
echo "Please run this script as USER ONLY."
echo ""
echo "Press control-c to quit..else the script will start in 1 seconds."
sleep 1
#sudo yum update
sudo yum install build-essential openssl-devel ssh expat libxyssl-dev libssl-dev dpkg make gcc
sudo yum remove subversion
sudo dpkg --purge subversion
#wget http://www.fayea.com/apache-mirror/subversion/subversion-1.7.5.tar.bz2
#bzip2 -d subversion-1.7.5.tar.bz2
#tar xvf subversion-1.7.5.tar
cd subversion-1.7.5
./get-deps.sh
cd neon/
./configure --prefix=/usr/local --with-ssl=openssl --with-pic
make
sudo make install
cd ..
#rm -rf neon
./configure --prefix=/usr/local --with-ssl=openssl --with-neon=/usr/local
make
sudo make install
cd ..
#rm -rf subversion-1.7.5
#rm subversion-1.6.11.tar.gz
#rm subversion-deps-1.6.11.tar.gz
exit 0