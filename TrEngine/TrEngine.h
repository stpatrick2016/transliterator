// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TRENGINE_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TRENGINE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef TRENGINE_EXPORTS
#define TRENGINE_API __declspec(dllexport)
#else
#define TRENGINE_API __declspec(dllimport)
#endif

BOOL RegisterKeyboardLayout(LPCTSTR filePath);
void EnableTransliteration();
void DisableTransliteration();
void SetCurrentLayout(int index);

typedef BOOL (*LPREGISTER_KEYBOARD_LAYOUT_ROUTINE)(LPCTSTR filePath);
typedef VOID (*LPENABLE_TRANSLITERATION_ROUTINE)();
typedef VOID (*LPSET_CURRENT_LAYOUT_ROUTINE)(int index);