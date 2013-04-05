// HookTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <conio.h>
#include "..\TrEngine\TrEngine.h"

int _tmain(int argc, _TCHAR* argv[])
{
	_tprintf(_T("Hit ENTER to exit \r\n"));

	HMODULE trLib = LoadLibrary(_T("TrEngine.dll"));
	LPREGISTER_KEYBOARD_LAYOUT_ROUTINE RegisterKeyboardLayout = (LPREGISTER_KEYBOARD_LAYOUT_ROUTINE)GetProcAddress(trLib, "RegisterKeyboardLayout");
	LPENABLE_TRANSLITERATION_ROUTINE EnableTransliteration = (LPENABLE_TRANSLITERATION_ROUTINE)GetProcAddress(trLib, "EnableTransliteration");
	LPENABLE_TRANSLITERATION_ROUTINE DisableTransliteration = (LPENABLE_TRANSLITERATION_ROUTINE)GetProcAddress(trLib, "DisableTransliteration");
	LPSET_CURRENT_LAYOUT_ROUTINE SetCurrentLayout = (LPSET_CURRENT_LAYOUT_ROUTINE)GetProcAddress(trLib, "SetCurrentLayout");
	
	RegisterKeyboardLayout(_T("Russian.kbd"));
	EnableTransliteration();
	SetCurrentLayout(0);

	_tprintf(_T("Hooking started. Press Alt-X to quit, Alt-E to enable hooking and Alt-D to disable hooking\r\n"));

	int commandChar = 0;
	do
	{
		commandChar = _getch();
		if(commandChar == L'e' || commandChar == L'E')
		{
			EnableTransliteration();
		}
		else if(commandChar == L'd' || commandChar == L'D')
		{
			DisableTransliteration();
		}
	}while(commandChar != L'x' && commandChar != L'X');

	_tprintf(_T("Sending QUIT signal to hook thread"));
	DisableTransliteration();
	return 0;
}



