// TrEngine.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "TrEngine.h"
#include "LayoutInfo.h"
#include <set>
#include <vector>
#include <string>

HHOOK g_hook = NULL;
HANDLE g_hookThread = NULL;
std::set<std::basic_string<WCHAR>> g_layoutsCache;
std::vector<CLayoutInfo*> g_layouts;
int g_currentLayout = -1;

LRESULT WINAPI KeyboardProc(int, WPARAM, LPARAM);
DWORD WINAPI HookThread(void*);
bool ProcessLayoutSwitch(PKBDLLHOOKSTRUCT info, WPARAM wParam);

////////////////////////////////////////////////////////////////
// Exported functions
//
BOOL WINAPI RegisterKeyboardLayout(LPCTSTR filePath)
{
	BOOL ret = FALSE;
	if(g_layoutsCache.find(filePath) == g_layoutsCache.end())
	{
		CLayoutInfo* l = new CLayoutInfo(filePath);
		if(l->IsLoaded())
		{
			g_layouts.push_back(l);
			ret = TRUE;
		}
		else
		{
			delete l;
		}
	}
	return ret;
}

void WINAPI EnableTransliteration()
{
	if(g_hookThread == NULL || GetThreadId(g_hookThread) == 0)
	{
		ATLTRACE(L"Starting keyboard hooking thread\r\n");
		DWORD threadId = 0;
		g_hookThread = CreateThread(NULL, 0, HookThread, NULL, 0, &threadId);
	}
	else
	{
		ATLTRACE(L"Hook thread already started. Ignoring subsequent request\r\n");
	}
}

void WINAPI DisableTransliteration()
{
	if(g_hookThread != NULL)
	{
		DWORD threadId = GetThreadId(g_hookThread);
		if(threadId != 0)
		{
			ATLTRACE(L"Sending STOP signal to hook thread\r\n");
			PostThreadMessage(threadId, WM_QUIT, 0, 0);
			if(WAIT_TIMEOUT == WaitForSingleObject(g_hookThread, 2000))
			{
				ATLTRACE(L"Thread is being aborted");
				TerminateThread(g_hookThread, 1);
			}

		}
		CloseHandle(g_hookThread);
		g_hookThread = NULL;
	}
}

BOOL WINAPI IsTransliterationEnabled()
{
	return g_hookThread != NULL;
}

void WINAPI SetCurrentLayout(int index)
{
	g_currentLayout = index;
	ATLTRACE(L"Current layout is changed to %d\r\n", index);
}

///////////////////////////////////////////////////////////////////
///Private methods and helpers
///
DWORD WINAPI HookThread(void*)
{
	g_hook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardProc, GetModuleHandle(NULL), 0);
	MSG  message;

   // Process messages until a quit message.
	while (GetMessage(&message,NULL,0,0))
	{
		TranslateMessage(&message);
		DispatchMessage(&message);

		if(message.message == WM_QUIT)
		{
			break;
		}
	}

	UnhookWindowsHookEx(g_hook);

	return 0;
}

LRESULT CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam) 
{
	bool processed = false;
	int layoutIndex = g_currentLayout;
	if (HC_ACTION == nCode && (wParam == WM_KEYDOWN || wParam == WM_KEYUP) ) 
	{
		PKBDLLHOOKSTRUCT info = (PKBDLLHOOKSTRUCT)lParam;
		if(!ProcessLayoutSwitch(info, wParam) && layoutIndex >= 0)
		{
			processed = g_layouts.at(layoutIndex)->ProcessEvent(info, wParam);
		}
	}

	if(!processed)
		return CallNextHookEx(g_hook, nCode,  wParam, lParam);
	else
		return 1;
}

bool ProcessLayoutSwitch(PKBDLLHOOKSTRUCT info, WPARAM wParam)
{
	//we should check for special key - in case user requested to switch to another layout or disable transliteration.
	//to disable transliteration, simply set g_currentLayout to -1 and return false.
	return false;
}
