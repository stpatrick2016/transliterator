#include "StdAfx.h"
#include <sstream>
#include <string>
#include <vector>
#include "LayoutInfo.h"

#define EXTRA_INFO_IDENTIFIER 13

CLayoutInfo::CLayoutInfo(LPCTSTR layoutPath)
{
	m_maxSourceLength = 0;
	m_isShiftPressed = m_isControlPressed = m_isAltPressed = m_isCapsLock = m_isWindowsKeyPressed = false;

	FILE* stream = NULL;
	if(_tfopen_s(&stream, layoutPath, L"r, ccs=UNICODE") == 0)
	{
		CharSequence* currentSequence = NULL;
		bool isTarget = false;
		bool isShift = false;
		std::basic_string<wchar_t> unicodeUnparsed;
		while(!feof(stream))
		{
			//read next character from file
			wint_t ch = fgetwc(stream);

			//in case this is end of line, close the current sequence
			if(ch == L'\r' || ch == L'\n')
			{
				//add to the list if we had a sequence
				if(currentSequence != NULL)
				{
					//fill in the shifted sequence if needed
					if(!isShift)
						currentSequence->ShiftTarget = currentSequence->Target;

					m_maxSourceLength = max((int)currentSequence->Source.size(), m_maxSourceLength);
					m_sequences[currentSequence->Source] = currentSequence;
					currentSequence = NULL;
				}

				//reset all kind of things
				isTarget = false;
				isShift = false;
			}

			//if this is the first "=", we stop processing source sequence and start target
			else if(ch == L'=' && isTarget == false)
			{
				//start processing the target from next character
				isTarget = true;
			}
			else if(isTarget == false)
			{
				//add a character to source
				if(currentSequence == NULL)
				{
					currentSequence = new CharSequence();
					unicodeUnparsed.clear();
				}
				
				DWORD scanCode = VkKeyScan(ch);
				currentSequence->Source.push_back(scanCode);
				m_acceptedCharacters.insert(scanCode);
			}
			else if(isTarget && ch == L'^')
			{
				isShift = true; //processing target when SHIFT is pressed
			}
			else
			{
				//this is finally the target
				unicodeUnparsed += ch;
				if(unicodeUnparsed.length() == 4)
				{
					//add the character itself
					WORD w;
					TCHAR* c;
					w = (WORD)_tcstol(unicodeUnparsed.c_str(), &c, 16);
					INPUT input = {0};
					input.type = INPUT_KEYBOARD;
					input.ki.dwFlags = KEYEVENTF_UNICODE;
					input.ki.wScan = w;
					input.ki.dwExtraInfo = EXTRA_INFO_IDENTIFIER;
					if(isShift)
						currentSequence->ShiftTarget.push_back(input);
					else
						currentSequence->Target.push_back(input);
					unicodeUnparsed.clear();
				}
			}
		}

		//in case last line didn't end with  EOLF, we add the rest to sequence.
		if(currentSequence != NULL)
		{
			//fill in the shifted sequence if needed
			if(!isShift)
				currentSequence->ShiftTarget = currentSequence->Target;
			m_maxSourceLength = max((int)currentSequence->Source.size(), m_maxSourceLength);
			m_sequences[currentSequence->Source] = currentSequence;
			currentSequence = NULL;
		}

		fclose(stream);
	}

	LoadSafeCharacters();
}

void CLayoutInfo::LoadSafeCharacters()
{
	m_safeCharacters.clear();
	m_safeCharacters.insert(VK_CANCEL); //ctrl+break
	m_safeCharacters.insert(VK_CONTROL);
	m_safeCharacters.insert(VK_SHIFT);
	m_safeCharacters.insert(VK_MENU); //alt
	m_safeCharacters.insert(VK_PAUSE);
	m_safeCharacters.insert(VK_CAPITAL); //caps lock
	m_safeCharacters.insert(VK_ESCAPE);
	m_safeCharacters.insert(VK_PRINT);
	m_safeCharacters.insert(VK_SNAPSHOT); //print screen
	m_safeCharacters.insert(VK_NUMLOCK);
	m_safeCharacters.insert(VK_SCROLL);
	m_safeCharacters.insert(VK_LSHIFT);
	m_safeCharacters.insert(VK_RSHIFT);
	m_safeCharacters.insert(VK_LCONTROL);
	m_safeCharacters.insert(VK_RCONTROL);
	m_safeCharacters.insert(VK_LMENU);
	m_safeCharacters.insert(VK_RMENU);
	m_safeCharacters.insert(VK_VOLUME_DOWN);
	m_safeCharacters.insert(VK_VOLUME_MUTE);
	m_safeCharacters.insert(VK_VOLUME_UP);
}


CLayoutInfo::~CLayoutInfo(void)
{
	SequenceMap::iterator it;
	for(it = m_sequences.begin(); it != m_sequences.end(); it++)
	{
		delete it->second;
		it++;
	}

	m_sequences.clear();
}

bool CLayoutInfo::ProcessEvent(PKBDLLHOOKSTRUCT keyInfo, WPARAM wParam)
{
	//we need to ignore the characters we sent
	if((keyInfo->flags & LLKHF_INJECTED) != 0 && keyInfo->dwExtraInfo == EXTRA_INFO_IDENTIFIER)
		return false;

	if(!ProcessSpecialKey(keyInfo, wParam))
		return false;

	if(m_isControlPressed || m_isAltPressed)
	{
		ATLTRACE(L"Control or Alt keys are pressed, ignoring the key");
		return false;
	}

	ProcessingInfo& inputBuffer = wParam == WM_KEYUP ? m_processingUp : m_processingDown;
	ATLTRACE(L"Selected sequence for %s\r\n", wParam == WM_KEYUP ? L"WM_KEYUP" : L"WM_KEYDOWN");

	//check if the character in the list of watched characters
	if(keyInfo->vkCode != VK_DELETE && m_acceptedCharacters.find(keyInfo->vkCode) == m_acceptedCharacters.end())
	{
		if(m_safeCharacters.find(keyInfo->vkCode) == m_safeCharacters.end())
		{
			ATLTRACE("Character %d not in the list and not in safe list, clearing the buffer\r\n", keyInfo->vkCode);
			inputBuffer.InputBuffer.clear(); //clear the buffer, since this is not one of the characters we are looking for
			inputBuffer.AmountSent = 0;
		}
		return false;
	}

	inputBuffer.InputBuffer.push_back(keyInfo->vkCode);
	if(inputBuffer.InputBuffer.size() > m_maxSourceLength)
		inputBuffer.InputBuffer.erase(inputBuffer.InputBuffer.begin());

#ifdef DEBUG
	ATLTRACE(L"Added character %d to sequence. New sequence is: ", keyInfo->vkCode);
	for(DWORD i=0; i<inputBuffer.InputBuffer.size(); i++)
	{
		ATLTRACE(L"%d ", inputBuffer.InputBuffer.at(i));
	}
	ATLTRACE(L"\r\n");
#endif

	VkCodeSequence seq = inputBuffer.InputBuffer;
	SequenceMap::iterator it;
	for(it=m_sequences.find(seq); it==m_sequences.end() && !seq.empty(); it=m_sequences.find(seq))
	{
		seq.erase(seq.begin());
	}

	if(it != m_sequences.end())
	{
		std::vector<INPUT> inputs;
		//sequence found, delete all previously sent characters
		unsigned int toDelete = inputBuffer.AmountSent > 0 ? min(inputBuffer.AmountSent, seq.size() - 1) : seq.size() - 1;
		ATLTRACE(L"Deleting last %d characters\r\n", toDelete);
		for(unsigned int i=0; i < toDelete; i++)
		{
			INPUT input = {0};
			input.type = INPUT_KEYBOARD;
			input.ki.wVk = VK_BACK; 
			input.ki.dwExtraInfo = EXTRA_INFO_IDENTIFIER;
			if(wParam == WM_KEYUP)
				input.ki.dwFlags = KEYEVENTF_KEYUP;
			inputs.push_back(input);
		}

		//let's send the replacement characters
		bool processShifted = m_isShiftPressed && !m_isCapsLock || m_isCapsLock && !m_isShiftPressed;
		inputBuffer.AmountSent = 0;
		std::vector<INPUT>::iterator inputIterator = processShifted ? it->second->ShiftTarget.begin() : it->second->Target.begin();
		for(; inputIterator != (processShifted ? it->second->ShiftTarget.end() : it->second->Target.end()); inputIterator++)
		{
			INPUT input = *inputIterator;
			if(wParam == WM_KEYUP)
				input.ki.dwFlags |= KEYEVENTF_KEYUP;
			inputs.push_back(input);
			inputBuffer.AmountSent++;
		}

		SendInput(inputs.size(), inputs.data(), sizeof(INPUT));
		return true;
	}
	else
	{
		inputBuffer.AmountSent = 0;
	}

	return false;
}

bool CLayoutInfo::ProcessSpecialKey(PKBDLLHOOKSTRUCT keyInfo, WPARAM wParam)
{
	bool ret = true;
	switch(keyInfo->vkCode)
	{
	case VK_LSHIFT:
	case VK_RSHIFT:
	case VK_SHIFT:
		m_isShiftPressed = wParam == WM_KEYDOWN;
		ATLTRACE(L"SHIFT pressed\r\n");
		ret = false;
		break;
	case VK_LMENU:
	case VK_RMENU:
	case VK_MENU:
		m_isAltPressed = wParam == WM_KEYDOWN;
		ATLTRACE(L"ALT pressed\r\n");
		ret = false;
		break;
	case VK_LCONTROL:
	case VK_RCONTROL:
	case VK_CONTROL:
		m_isControlPressed = wParam == WM_KEYDOWN;
		ATLTRACE(L"CTRL pressed\r\n");
		ret = false;
		break;
	case VK_CAPITAL:
		if(wParam == WM_KEYUP)
		{
			m_isCapsLock = !m_isCapsLock;
			ATLTRACE(L"CAPSLOCK pressed\r\n");
		}
		ret = false;
		break;
	case VK_LWIN:
	case VK_RWIN:
		m_isWindowsKeyPressed = wParam == WM_KEYDOWN;
		ATLTRACE(L"Windows Key pressed\r\n");
		ret = false;
		break;
	}

	return ret;
}
