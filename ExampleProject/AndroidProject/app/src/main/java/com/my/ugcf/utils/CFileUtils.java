package com.my.ugcf.utils;

import android.os.Environment;
import android.util.Log;

import java.io.File;
import java.io.RandomAccessFile;

public class CFileUtils {
    private static String rootFolderName = "Eletell";
    private static String externalName = Environment.getExternalStorageDirectory().getAbsolutePath();

    public static String GetEletellFolder()
    {
        String fullPath = externalName + File.separator + rootFolderName + File.separator;
        File f = new File(fullPath);
        if(!f.exists()){
            f.mkdirs();
        }
        return fullPath;
    }

    public static void DeleteFile(String fullPath)
    {
        File f = new File(fullPath);
        if(f.exists())
            f.delete();
    }

    public static String CreateFile(String content, String filePath, String fileName)   {
        //生成文件夹之后，再生成文件，不然会出错
        String strFilePath = MakeFilePath(filePath, fileName);
        // 每次写入时，都换行写
        String strContent = content + "\r\n";
        try {
            File file = new File(strFilePath);
            if (!file.exists()) {
                Log.d("eletell", "Create the file:" + strFilePath);
                file.getParentFile().mkdirs();
                file.createNewFile();
            }
            RandomAccessFile raf = new RandomAccessFile(file, "rwd");
            raf.seek(file.length());
            raf.write(strContent.getBytes());
            raf.close();
        } catch (Exception e) {
            Log.e("TestFile", "Error on write File:" + e);
        }
        return strFilePath;
    }

    // 生成文件
    public static String MakeFilePath(String filePath, String fileName) {
        File file = null;
        String fullFilePath = GetEletellFolder() + filePath + File.separator;
        File dir = new File(fullFilePath);
        if (!dir.exists())
        {
            dir.mkdirs();
        }
        fullFilePath = fullFilePath + fileName;
        try {
            file = new File(fullFilePath);
            if (!file.exists()) {
                file.createNewFile();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return fullFilePath;
    }
}
