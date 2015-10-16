// encodeDB.cpp 
//

#include "sqlite3.h"
//#include "sqlite3ext.h"
#include <string>
#include <stdio.h>
#include<iostream>
using namespace std;
int main(int argc, char* argv[])
{
	
	if(argc<2)
	{
		printf("need full path\n");
		return 0;
	}
	char* src = argv[1];
	char* AES_KEY1 = argv[2];
	sqlite3 * db=NULL;
	if (sqlite3_open(src, &db) == SQLITE_OK) {
		int result = sqlite3_rekey(db,AES_KEY1,strlen(AES_KEY1));
		printf("sqlite3_key %d",result);
		if(result!=SQLITE_OK)
		{
			return 0;
		} 
		//sqlite3_stmt *stmt;
		//int ok=sqlite3_prepare_v2(db, "select * from ai", -1, &stmt, NULL);
		sqlite3_close(db);
	}
	else
	{
		printf("open db failed\n");
	}
	return 0;
}

