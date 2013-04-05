// HookTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "LayoutInfo.h"

HHOOK g_hook = NULL;
CLayoutInfo* g_currentLayout = NULL;

LRESULT WINAPI KeyboardProc(int, WPARAM, LPARAM);
DWORD WINAPI HookThread(void*);

int _tmain(int argc, _TCHAR* argv[])
{
	_tprintf(_T("Hit ENTER to exit \r\n"));

	g_currentLayout = new CLayoutInfo(_T("Russian.kbd"));

	DWORD threadId = 0;
	HANDLE thread = CreateThread(NULL, 0, HookThread, NULL, 0, &threadId);
	
	getchar();
	_tprintf(_T("Sending QUIT signal to hook thread"));
	PostThreadMessage(threadId, WM_QUIT, 0, 0);
	if(WAIT_TIMEOUT == WaitForSingleObject(thread, 2000))
	{
		_tprintf(_T("Aborting thread"));
	}

	CloseHandle(thread);
	return 0;
}

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
	static const LCID Locale = MAKELCID(0x0419, SORT_DEFAULT);
	bool processed = false;
	if (HC_ACTION == nCode && (wParam == WM_KEYDOWN || wParam == WM_KEYUP) && g_currentLayout != NULL) 
	{
		PKBDLLHOOKSTRUCT info = (PKBDLLHOOKSTRUCT)lParam;
		_tprintf(_T("KEYBOARD -WPARAM: %s, vk: %d, scan: %d, flags: %d\r\n"), wParam == WM_KEYUP ? L"WM_KEYUP  " : L"WM_KEYDOWN", info->vkCode, info->scanCode, info->flags);
		processed = g_currentLayout->ProcessEvent(info, wParam);
	}

	if(!processed)
		return CallNextHookEx(g_hook, nCode,  wParam, lParam);
	else
		return 1;
}



