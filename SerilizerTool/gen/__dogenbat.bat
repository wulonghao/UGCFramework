@echo off

echo ^@echo off
echo.
echo cd ..\
echo protobuf-net\ProtoGen\protogen.exe ^^
cd ..\
for %%i in (*.proto) do echo -i:%%i ^^
cd gen
echo -o:ProtobufModel\ProtobufModel.cs -ns:ProtobufModel
echo cd gen