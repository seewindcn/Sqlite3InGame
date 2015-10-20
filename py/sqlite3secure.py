#!/usr/bin/env python
#encoding=utf-8

import os
import sys
from os.path import abspath, dirname, join
import cffi
import glob

curpath = abspath(dirname(__file__))

libsqlite3secure = None
ffi = None
def init(findpath=None):
    global libsqlite3secure, ffi
    if findpath == None: findpath = curpath
    if sys.platform.startswith('cygwin'):
        search_string = os.path.join(findpath, 'sqlite3.dll')
    else:
        search_string = os.path.join(findpath, 'sqlite3.so')

    flist = glob.glob(search_string)
    soname = flist[0]
    print('sqlite3:', soname)
    ffi = cffi.FFI()
    # ffi.set_source("sqlite3", "include <sqlite3.h>", include_dirs=[findpath])
    ffi.cdef('''
    typedef struct sqlite3 sqlite3;
    int sqlite3_open(
      const char *filename,   /* Database filename (UTF-8) */
      sqlite3 **ppDb          /* OUT: SQLite db handle */
    );
    int sqlite3_open16(
      const void *filename,   /* Database filename (UTF-16) */
      sqlite3 **ppDb          /* OUT: SQLite db handle */
    );
    int sqlite3_open_v2(
      const char *filename,   /* Database filename (UTF-8) */
      sqlite3 **ppDb,         /* OUT: SQLite db handle */
      int flags,              /* Flags */
      const char *zVfs        /* Name of VFS module to use */
    );
    int sqlite3_rekey(
      sqlite3 *db,                   /* Database to be rekeyed */
      const void *pKey, int nKey     /* The new key */
    );
    int sqlite3_close(sqlite3*);
    int sqlite3_close_v2(sqlite3*);
    ''')
    libsqlite3secure = ffi.dlopen(soname)
    return libsqlite3secure

def test():
    dbpath = join(curpath, 'db', 'res1.db')
    init(abspath('sqlite3'))
    print(libsqlite3secure)
    ppDb = ffi.new('sqlite3**')
    print("sqlite3_open", libsqlite3secure.sqlite3_open(dbpath, ppDb))
    print ("sqlite3_rekey", libsqlite3secure.sqlite3_rekey(ppDb[0], "1234567890123456", 16))
    libsqlite3secure.sqlite3_close(ppDb[0])
    #import IPython;IPython.embed()

def main():
    test()

if __name__ == '__main__':
    main()
