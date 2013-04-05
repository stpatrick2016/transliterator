#pragma once

#include <map>
#include <vector>
#include <set>

class CLayoutInfo
{
public:
	CLayoutInfo(LPCTSTR layoutPath);
	~CLayoutInfo(void);

	bool ProcessEvent(PKBDLLHOOKSTRUCT keyInfo, WPARAM wParam);
	bool IsLoaded() {return m_isLoaded;}

private:

	//helper methods
	void LoadSafeCharacters();
	bool ProcessSpecialKey(PKBDLLHOOKSTRUCT keyInfo, WPARAM wParam);

	//type definitions
	typedef std::vector<DWORD> VkCodeSequence;
	typedef struct {
		VkCodeSequence Source;
		std::vector<INPUT> Target;
		std::vector<INPUT> ShiftTarget;
	} CharSequence;

	typedef struct ProcessingInfo {
		VkCodeSequence InputBuffer;
		unsigned int AmountSent;

		ProcessingInfo()
		{
			AmountSent = 0;
		}
	};

	struct vk_less
	{
		bool operator() (const VkCodeSequence& v1, const VkCodeSequence& v2) const
		{
			if(v1.size() == v2.size())
			{
				for(unsigned int i=0; i<v1.size(); i++)
				{
					if(v1.at(i) != v2.at(i))
						return v1.at(i) < v2.at(i);
				}

				return false;
			}
			else
			{
				return v1.size() < v2.size();
			}
		}
	};

	typedef std::map<VkCodeSequence, CharSequence*, vk_less> SequenceMap;

	//private members
	bool m_isLoaded;
	SequenceMap m_sequences;
	unsigned int m_maxSourceLength;
	ProcessingInfo m_processingUp;
	ProcessingInfo m_processingDown;
	std::set<DWORD> m_acceptedCharacters;
	std::set<DWORD> m_safeCharacters;
	bool m_isShiftPressed,
		 m_isControlPressed,
		 m_isAltPressed,
		 m_isCapsLock;
};

