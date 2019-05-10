CSC			= /cygdrive/c/Windows/Microsoft.NET/Framework/v4.0.30319/csc.exe
TARGET		= osz_installer.exe
SRC			= main.cs
BIN_DIR		= bin_
ZIP_DIR 	= zip
TMP_DIR 	= tmp
PRJ_NAME	= osz_installer

FRAMEWORK_DIR_W	= C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\
CSC_FLAGS		= /nologo \
	/utf8output \
	/win32icon:res\\osz_installer.ico \
	/r:$(FRAMEWORK_DIR_W)System.IO.Compression.ZipFile.dll \
	/r:$(FRAMEWORK_DIR_W)System.IO.Compression.FileSystem.dll \
	/r:$(FRAMEWORK_DIR_W)netstandard.dll

DEBUG_FLAGS		= 
RELEASE_FLAGS	= /t:winexe

.PHONY: debug
debug: BIN_DIR=bin_debug
debug: CSC_FLAGS+=$(DEBUG_FLAGS)
debug: build

.PHONY: release
release: BIN_DIR=bin_release
release: CSC_FLAGS+=$(RELEASE_FLAGS)
release: build
release: gen_zip

.PHONY: gen_zip
gen_zip:
	-rm -rf $(TMP_DIR)
	mkdir $(TMP_DIR)
	mkdir $(TMP_DIR)/$(PRJ_NAME)/
	mkdir $(TMP_DIR)/$(PRJ_NAME)/src/
	mkdir $(TMP_DIR)/$(PRJ_NAME)/bin/
	cp \
		README.md \
		$(TMP_DIR)/$(PRJ_NAME)/
	cp \
		LICENSE \
		$(TMP_DIR)/$(PRJ_NAME)/
	cp \
		$(SRC) \
		$(TMP_DIR)/$(PRJ_NAME)/src/
	cp \
		$(BIN_DIR)/*.exe \
		$(TMP_DIR)/$(PRJ_NAME)/bin/
	cp \
		configure.sample.ini \
		$(TMP_DIR)/$(PRJ_NAME)/bin/
	-mkdir $(ZIP_DIR)
	cd $(TMP_DIR) && zip \
		-r ../$(ZIP_DIR)/$(PRJ_NAME)_$(shell date +%Y%m%d%H%M).zip \
		.
	rm -rf $(TMP_DIR)

.PHONY: build
build: main.cs
	-mkdir $(BIN_DIR)
	$(CSC) \
		$(CSC_FLAGS) \
		/out:$(BIN_DIR)/$(TARGET) \
		$(SRC)

clean:
	-rm $(BIN_DIR)*/*.exe

