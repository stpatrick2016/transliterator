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
BOOL RegisterKeyboardLayout(LPCTSTR filePath)
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

void EnableTransliteration()
{
	if(g_hookThread == NULL || GetThreadId(g_hookThread) == 0)
	{
		DWORD threadId = 0;
		g_hookThread = CreateThread(NULL, 0, HookThread, NULL, 0, &threadId);
	}
}

void DisableTransliteration()
{
	if(g_hookThread != NULL)
	{
		DWORD threadId = GetThreadId(g_hookThread);
		if(threadId != 0)
		{
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
	if (HC_ACTION == nCode && (wParam == WM_KEYDOWN || wParam == WM_KEYUP) && layoutIndex >= 0) 
	{
		PKBDLLHOOKSTRUCT info = (PKBDLLHOOKSTRUCT)lParam;
		if(!ProcessLayoutSwitch(info, wParam))
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
	return false;
}
