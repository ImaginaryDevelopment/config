for instance c:\users\username
type files

c:\projects\config\userprofile>mklink /h .bashrc "%userprofile%\.bashrc"
c:\projects\config\userprofile>mklink /h .gitconfig "%userprofile%\.gitconfig"
c:\projects\config\userprofile>mklink /j Roaming "%userprofile%/appdata/roaming"
