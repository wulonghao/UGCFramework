@echo off

cd ..\
protobuf-net\ProtoGen\protogen.exe ^
-i:Game.proto ^
-o:ProtobufModel\ProtobufModel.cs -ns:ProtobufModel
cd gen
