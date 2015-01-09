#!/bin/sh
#
# This hook prevents you from committing any file containing debug code.
# e.g. dsm(), dpm(), alert() and console.log(). There is also a PHP LINT check
# to ensure your syntax is okay.
#
# To enable this hook, symlink it (run this from the root of the repository).
#
# ln -s ../../scripts/pre-commit.sh .git/hooks/pre-commit
#
# To force a commit that breaks the below rules (e.g. when debug code is 100%
# required you can add in another parameter to `git commit` namely `--no-verify`.
#
# Helpful git aliases for these are:
# git config --global alias.gc commit
# git config --global alias.gcv commit --no-verify
# from https://raw.githubusercontent.com/wiifm69/drupal-pre-commit/master/scripts/pre-commit.sh
# n.b. pwd is always the working copy's root directory.

# Non-zero exit aborts the commit
ABORT_COMMIT=1

# Get the list of modified files, excluding deleted files (status 'D')
# as obviously we cannot checkout and examine those.
oldIFS="$IFS"

IFS=$'\r\n'
DIFF_FILES=$(git diff-index HEAD --cached --name-status | grep -v ^D | cut -f2-)
#DIFF_Array=($(${DIFF_FILES//$'\r\n'/ }))

if [ $? -ne 0 ]; then
  echo "Error getting list of changed files in pre-commit hook"
  exit $ABORT_COMMIT
fi

# Assume success.
EXIT=0

# Make a directory, under which we will checkout the staged versions of the
# files we are about to commit. We need to do this so that we are actually
# testing the code which will be committed. (If we tested against the working
# copy, we could have syntax errors in the staged version with inadvertantly-
# unstaged fixes, and we wouldn't catch the problem.)
STAGED=".validate_pre_commit"
mkdir -p "$STAGED"

## Truncate the LINT error log file
#LINTLOG="$STAGED/lint.log"
##>$LINTLOG

# LINQ code checks.
LinqFiles="\.(linq)$"
FUNCTIONS="<Server>|SRV|FSharpProgram"
PATTERN="($FUNCTIONS)"
#echo "diff files coming"
#echo $DIFF_FILES
#echo "diff files came"

echo $GIT_WORK_TREE

for FILE in ${DIFF_FILES//$'\r\n'/ }; do
	#echo "staged file to check: \"$FILE\""
  	PARSEABLE=$(echo "$FILE" | grep -E "$LinqFiles");
  	#echo "about to echo parseable"
  	#echo "$PARSEABLE"
  	#echo "parseable has run, checking it"
	if [ "$PARSEABLE" != "" ]; then
		#echo $FILE
		#echo "git checkout-index -f --prefix=\"$STAGED/\" \"$FILE\" "
    	git checkout-index -f --prefix="$STAGED/" "$FILE"

		#echo "cat \"$STAGED/$FILE\""
		#http://stackoverflow.com/questions/1221833/bash-pipe-output-and-capture-exit-status
		# -i to ignore case on grep/egrep
		#echo "cat \"$STAGED/$FILE\" | egrep -Ehn -C0 \"$PATTERN\""
		egrepoutput=$(cat "$STAGED/$FILE" | egrep -Ehn -C0 "$PATTERN")
		if [ "$egrepoutput" != "" ]; then
			echo $egrepoutput
			#echo $?
			echo "---------------------------------------"
			echo "^ Found blacklisted code in $FILE"
			echo "---------------------------------------"
			EXIT=$ABORT_COMMIT # but still run the remaining tests
		fi
    	rm -f "$STAGED/$FILE"
	fi
done


if [ $EXIT -ne 0 ]; then
  echo $'\ngit: Can\'t commit; fix errors first.'
  echo "(If you definitely need to commit this as-is, use the --no-verify option.)"
  echo $'\nIf the reported line numbers do not match, try stashing your unstaged changes:'
  echo $'git stash save --keep-index\n'
fi

# Clean up the directory, if preferred:
#rm -f $LINTLOG
find "$STAGED" -type d -print0 | xargs -0r rmdir -p --ignore-fail-on-non-empty
#temp while we check this script
exit 1
exit $EXIT