#include <stdlib.h>
#include <windows.h>
#include <iostream>
#include <ctime> // For rand() function
using namespace std;

// Define virtual key codes for keyboard inputs
#define VK_A 0x41
#define VK_B 0x42
#define VK_C 0x43
#define VK_D 0x44
#define VK_E 0x45
#define VK_F 0x46
#define VK_G 0x47
#define VK_H 0x48
#define VK_I 0x49
#define VK_J 0x4A
#define VK_K 0x4B
#define VK_L 0x4C
#define VK_M 0x4D
#define VK_N 0x4E
#define VK_O 0x4F
#define VK_P 0x50
#define VK_Q 0x51
#define VK_R 0x52
#define VK_S 0x53
#define VK_T 0x54
#define VK_U 0x55
#define VK_V 0x56
#define VK_W 0x57
#define VK_X 0x58
#define VK_Y 0x59
#define VK_Z 0x5A

// Structure to hold game information
struct GAMEINFO {
	COORD PlayerOnePosition;
	COORD PlayerTwoPosition;
	COORD PlayerOneBullet;
	COORD PlayerTwoBullet;
	COORD PlayerOneBullet2;
	COORD PlayerTwoBullet2;
	COORD ZeroZero;
	vector<COORD> PlayerOneBullets;
	vector<COORD> PlayerTwoBullets;
	vector<COORD> PowerUps;
	vector<COORD> Obstacles;
	int PlayerOneHealth;
	int PlayerTwoHealth;
	int PlayerOneSpecialCharge;
	int PlayerTwoSpecialCharge;
	int PlayerOneWeaponType;
	int PlayerTwoWeaponType;
	float PlayerOneSpeed;
	float PlayerTwoSpeed;
	string CurrentWeather;
};

HANDLE hInput, hOutput; // Handles for input and output
GAMEINFO GameInfo; // Game information instance

// Additional global variables for game state
int scorePlayerOne = 0;
int scorePlayerTwo = 0;
bool isAIEnabled = true; // Toggle AI for player two

// Function prototypes
void Movement(GAMEINFO &GameInfo);
void Draw(GAMEINFO);
void Erase(GAMEINFO);
int LaunchBullet(GAMEINFO &GameInfo, int);
void LaunchBullet2(GAMEINFO &GameInfo, int);
int Wait();
void UpdateScore(int player);
void AIBehavior(GAMEINFO &GameInfo);
void CheckBulletCollision(GAMEINFO &GameInfo);
void ApplyEnvironmentalEffects(GAMEINFO &GameInfo);
void LevelUpPlayer(GAMEINFO &GameInfo, int player);
void FireWeapon(GAMEINFO &GameInfo, int player);
void UpdateObstacles(GAMEINFO &GameInfo);
void SwitchWeapon(GAMEINFO &GameInfo, int player, int weaponType);

// Displayed at the top of the console.
void DrawInitialUI()
{
    COORD uiPos = {0, 0}; // Position for UI display
    SetConsoleCursorPosition(hOutput, uiPos);
    cout << "Player 1 Score: " << scorePlayerOne << " | Player 2 Score: " << scorePlayerTwo;
}

int main()
{
    // Initialize game information and console handles
	hInput = GetStdHandle(STD_INPUT_HANDLE);
	hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
		
	SetConsoleMode(hOutput, ENABLE_PROCESSED_INPUT);
	// Set initial positions for players and bullets
	GameInfo.PlayerOnePosition.X = 19;
	GameInfo.PlayerOnePosition.Y = 12;
	GameInfo.PlayerTwoPosition.X = 61;
	GameInfo.PlayerTwoPosition.Y = 12;
	GameInfo.PlayerOneBullet.X = 0;
	GameInfo.PlayerOneBullet.Y = 0;
	GameInfo.PlayerTwoBullet.X = 79;
	GameInfo.PlayerTwoBullet.Y = 0;
	GameInfo.PlayerOneBullet2.X = 1;
	GameInfo.PlayerOneBullet2.Y = 0;
	GameInfo.PlayerTwoBullet2.X = 78;
	GameInfo.PlayerTwoBullet2.Y = 0;
	GameInfo.ZeroZero.X = 0;
	GameInfo.ZeroZero.Y = 0;

	int i;
	GameInfo.ZeroZero.Y = 24;
	for(i = 0; i < 79; i++){
		SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
		cout << ".";
		GameInfo.ZeroZero.X++;
	}
    // Draw the initial game state
	Draw(GameInfo);

    // Initial UI setup
    DrawInitialUI();

    // Main game loop
    while(1){
        ApplyEnvironmentalEffects(GameInfo);
        Movement(GameInfo);
        UpdateBullets(GameInfo);
        UpdateObstacles(GameInfo);
        CheckBulletCollision(GameInfo);
        Draw(GameInfo);
        Sleep(100); // Add delay to make game movements smoother and more manageable
    }
	
	return 0;
}

void Movement(GAMEINFO &GameInfo)
{	
	INPUT_RECORD InputRecord;
	DWORD Events = 0;

    // Read input from the console
	ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    // Process keyboard events
    if(InputRecord.EventType == KEY_EVENT){
        // Player 1 movement and shooting
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y--;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y++;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown){
            FireWeapon(GameInfo, 1);
        }

        // Player 2 movement and shooting (handled by AI if enabled)
        AIBehavior(GameInfo);

        // Check for bullet collisions
        CheckBulletCollision(GameInfo);
    }
}

void Draw(GAMEINFO GameInfo)
{
    // Draw player one
	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
	cout << "|";
    // Draw player two
	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
	cout << "|";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
	cout << ".";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
	cout << ".";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
	cout << ".";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
	cout << ".";
	
	GameInfo.ZeroZero.X = 0;
	GameInfo.ZeroZero.Y = 0;

	int i;
	for(i = 0; i < 79; i++){
		SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
		cout << ".";
		GameInfo.ZeroZero.X++;
	}

    // Draw all bullets
    for (auto &bullet : GameInfo.PlayerOneBullets) {
        SetConsoleCursorPosition(hOutput, bullet);
        cout << '*';
    }
    for (auto &bullet : GameInfo.PlayerTwoBullets) {
        SetConsoleCursorPosition(hOutput, bullet);
        cout << '*';
    }

    // Draw all power-ups
    for (auto &powerUp : GameInfo.PowerUps) {
        SetConsoleCursorPosition(hOutput, powerUp);
        cout << '$'; // Display power-up as $
    }

    // Draw obstacles
    for (auto &obstacle : GameInfo.Obstacles) {
        SetConsoleCursorPosition(hOutput, obstacle);
        cout << '#'; // Display obstacles as #
    }
}

void Erase(GAMEINFO GameInfo)
{
    // Erase player one
	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
	cout << " ";
	// Erase player two
	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
	cout << " ";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
	cout << " ";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
	cout << " ";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
	cout << " ";

	SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
	cout << " ";
}

int LaunchBullet(GAMEINFO &GameInfo, int PlayerNumber)
{	

	int i;
    // Launch bullet for player one
	if(PlayerNumber == 1){
		GameInfo.PlayerOneBullet.Y = GameInfo.PlayerOnePosition.Y;
		GameInfo.PlayerOneBullet.X = GameInfo.PlayerOnePosition.X + 1;
		Draw(GameInfo);
		Erase(GameInfo);
		for(i = 0; i < 77; i++){
			GameInfo.PlayerOneBullet.X += 1;
			Draw(GameInfo);
	
			int move;
			move =	Wait();
			switch(move){
			case 1:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y--;
				if(GameInfo.PlayerOnePosition.Y < 0)
					GameInfo.PlayerOnePosition.Y++;
				break;
			case 2:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y++;
				if(GameInfo.PlayerOnePosition.Y > 24)
					GameInfo.PlayerOnePosition.Y--;
				break;
			case 3:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y--;
				if(GameInfo.PlayerTwoPosition.Y < 0)
					GameInfo.PlayerTwoPosition.Y++;
				break;
			case 4:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y++;
				if(GameInfo.PlayerTwoPosition.Y > 24)
					GameInfo.PlayerTwoPosition.Y--;
				break;
			case 5:
				LaunchBullet2(GameInfo, 1);
				return 0;
				break;
			case 6:
				LaunchBullet2(GameInfo, 2);
				return 0;
				break;
			}

			Draw(GameInfo);
			Erase(GameInfo);
			if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
				if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
					system("cls");
					cout << "\aPlayer 1 Wins" << endl;
					system("pause");
					exit(0);
				}
		}
		GameInfo.PlayerOneBullet.Y = 0;
		GameInfo.PlayerOneBullet.X = 0;
		Draw(GameInfo);
	}
	if(PlayerNumber == 2){
		GameInfo.PlayerTwoBullet.Y = GameInfo.PlayerTwoPosition.Y;
		GameInfo.PlayerTwoBullet.X = GameInfo.PlayerTwoPosition.X - 1;
		Draw(GameInfo);
		Erase(GameInfo);
		for(i = 0; i < 77; i++){
			GameInfo.PlayerTwoBullet.X -= 1;
			Draw(GameInfo);
		
			int move;
			move =	Wait();
			switch(move){
			case 1:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y--;
				if(GameInfo.PlayerOnePosition.Y < 0)
					GameInfo.PlayerOnePosition.Y++;
				break;
			case 2:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y++;
				if(GameInfo.PlayerOnePosition.Y > 24)
					GameInfo.PlayerOnePosition.Y--;
				break;
			case 3:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y--;
				if(GameInfo.PlayerTwoPosition.Y < 0)
					GameInfo.PlayerTwoPosition.Y++;
				break;
			case 4:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y++;
				if(GameInfo.PlayerTwoPosition.Y > 24)
					GameInfo.PlayerTwoPosition.Y--;
				break;
			case 5:
				LaunchBullet2(GameInfo, 1);
				return 0;
				break;
			case 6:
				LaunchBullet2(GameInfo, 2);
				return 0;
				break;
			}
			
			Draw(GameInfo);
			Erase(GameInfo);
			if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
				if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
					system("cls");
					cout << "\aPlayer 2 Wins" << endl;
					system("pause");
					exit(0);
			}
		}
		GameInfo.PlayerTwoBullet.Y = 0;
		GameInfo.PlayerTwoBullet.X = 79;
		Draw(GameInfo);
	}
	return 0;
}

int Wait()
{	
	INPUT_RECORD InputRecord;
	DWORD Events = 0;
	
	if(WAIT_TIMEOUT == WaitForSingleObject(hInput,1))
				return 0;
	ReadConsoleInput(hInput, &InputRecord, 1, &Events);

	if(InputRecord.EventType == KEY_EVENT){
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 1;
		
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 2;
		
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_O && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 3;
		
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_L && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 4;
		
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 5;
		
		if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_K && InputRecord.Event.KeyEvent.bKeyDown == 1)
			return 6;
	}
	FlushConsoleInputBuffer(hInput);
	return 0;
}

void LaunchBullet2(GAMEINFO &GameInfo, int PlayerNumber)
{
	if(PlayerNumber == 1){
		GameInfo.PlayerOneBullet2.X = GameInfo.PlayerOnePosition.X + 1;
		GameInfo.PlayerOneBullet2.Y = GameInfo.PlayerOnePosition.Y;

		Draw(GameInfo);
		Erase(GameInfo);
		int i;
		for(i = 0; i < 77; i++){
			GameInfo.PlayerOneBullet.X += 1;
			GameInfo.PlayerOneBullet2.X += 1;
			GameInfo.PlayerTwoBullet.X -= 1;
			GameInfo.PlayerTwoBullet2.X -= 1;
			Draw(GameInfo);
	
			int move;
			move =	Wait();
			switch(move){
			case 1:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y--;
				if(GameInfo.PlayerOnePosition.Y < 0)
					GameInfo.PlayerOnePosition.Y++;
				break;
			case 2:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y++;
				if(GameInfo.PlayerOnePosition.Y > 24)
					GameInfo.PlayerOnePosition.Y--;
				break;
			case 3:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y--;
				if(GameInfo.PlayerTwoPosition.Y < 0)
					GameInfo.PlayerTwoPosition.Y++;
				break;
			case 4:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y++;
				if(GameInfo.PlayerTwoPosition.Y > 24)
					GameInfo.PlayerTwoPosition.Y--;
				break;
			}

			Draw(GameInfo);
			Erase(GameInfo);
			if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
				if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
					system("cls");
					cout << "\aPlayer 1 Wins" << endl;
					system("pause");
					exit(0);
				}
			if(GameInfo.PlayerOneBullet2.X == GameInfo.PlayerTwoPosition.X)
				if(GameInfo.PlayerOneBullet2.Y == GameInfo.PlayerTwoPosition.Y){
					system("cls");
					cout << "\aPlayer 1 Wins" << endl;
					system("pause");
					exit(0);
				}
		}
		GameInfo.PlayerOneBullet.Y = 0;
		GameInfo.PlayerOneBullet.X = 0;
		GameInfo.PlayerOneBullet2.Y = 0;
		GameInfo.PlayerOneBullet2.X = 1;
		Draw(GameInfo);
	}

	if(PlayerNumber == 2){
		GameInfo.PlayerTwoBullet2.Y = GameInfo.PlayerTwoPosition.Y;
		GameInfo.PlayerTwoBullet2.X = GameInfo.PlayerTwoPosition.X - 1;
		Draw(GameInfo);
		Erase(GameInfo);
		int i;
		for(i = 0; i < 77; i++){
			GameInfo.PlayerTwoBullet.X -= 1;
			GameInfo.PlayerTwoBullet2.X -= 1;
			GameInfo.PlayerOneBullet.X += 1;
			GameInfo.PlayerOneBullet2.X += 1;
			Draw(GameInfo);
		
			int move;
			move =	Wait();
			switch(move){
			case 1:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y--;
				if(GameInfo.PlayerOnePosition.Y < 0)
					GameInfo.PlayerOnePosition.Y++;
				break;
			case 2:
				Erase(GameInfo);
				GameInfo.PlayerOnePosition.Y++;
				if(GameInfo.PlayerOnePosition.Y > 24)
					GameInfo.PlayerOnePosition.Y--;
				break;
			case 3:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y--;
				if(GameInfo.PlayerTwoPosition.Y < 0)
					GameInfo.PlayerTwoPosition.Y++;
				break;
			case 4:
				Erase(GameInfo);
				GameInfo.PlayerTwoPosition.Y++;
				if(GameInfo.PlayerTwoPosition.Y > 24)
					GameInfo.PlayerTwoPosition.Y--;
				break;
			}

			Draw(GameInfo);
			Erase(GameInfo);
			if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
				if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
					system("cls");
					cout << "\aPlayer 2 Wins" << endl;
					system("pause");
					exit(0);
			}
			if(GameInfo.PlayerTwoBullet2.X == GameInfo.PlayerOnePosition.X)
				if(GameInfo.PlayerTwoBullet2.Y == GameInfo.PlayerOnePosition.Y){
					system("cls");
					cout << "\aPlayer 2 Wins" << endl;
					system("pause");
					exit(0);
			}
		}
		GameInfo.PlayerOneBullet.Y = 0;
		GameInfo.PlayerOneBullet.X = 0;
		GameInfo.PlayerOneBullet2.Y = 0;
		GameInfo.PlayerOneBullet2.X = 1;
		Draw(GameInfo);
	}
}

void UpdateScore(int player)
{
    // Update score based on player number
    if(player == 1)
        scorePlayerOne++;
    else if(player == 2)
        scorePlayerTwo++;

    // Display updated scores
    COORD scorePos = {0, 1}; // Position for score display
    SetConsoleCursorPosition(hOutput, scorePos);
    cout << "Player 1 Score: " << scorePlayerOne << " | Player 2 Score: " << scorePlayerTwo;
}

void AIBehavior(GAMEINFO &GameInfo)
{
    // Simple AI for player two
    if(isAIEnabled){
        // Randomly decide to move up or down
        int decision = rand() % 2;
        if(decision == 0 && GameInfo.PlayerTwoPosition.Y > 0){ // Move up
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y--;
            Draw(GameInfo);
        } else if(GameInfo.PlayerTwoPosition.Y < 24){ // Move down
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y++;
            Draw(GameInfo);
        }
    }
}

void CheckBulletCollision(GAMEINFO &GameInfo)
{
    // Check if player one's bullet hits player two
    if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X && GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
        UpdateScore(1); // Player one scores
        GameInfo.PlayerOneBullet.Y = 0; // Reset bullet position
    }
    // Check if player two's bullet hits player one
    if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X && GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
        UpdateScore(2); // Player two scores
        GameInfo.PlayerTwoBullet.Y = 0; // Reset bullet position
    }
}

void Movement(GAMEINFO &GameInfo)
{
    INPUT_RECORD InputRecord;
    DWORD Events = 0;

    // Read input from the console
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    // Process keyboard events
    if(InputRecord.EventType == KEY_EVENT){
        // Player 1 movement and shooting
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y--;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y++;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown){
            LaunchBullet(GameInfo, 1);
        }

        // Player 2 movement and shooting (handled by AI if enabled)
        AIBehavior(GameInfo);

        // Check for bullet collisions
        CheckBulletCollision(GameInfo);
    }
}

void Draw(GAMEINFO GameInfo)
{
    // Draw player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << "|";
    // Draw player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << "|";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << ".";
    
    GameInfo.ZeroZero.X = 0;
    GameInfo.ZeroZero.Y = 0;

    int i;
    for(i = 0; i < 79; i++){
        SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
        cout << ".";
        GameInfo.ZeroZero.X++;
    }

}

void Erase(GAMEINFO GameInfo)
{
    // Erase player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << " ";
    // Erase player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << " ";
}

int LaunchBullet(GAMEINFO &GameInfo, int PlayerNumber)
{   

    int i;
    // Launch bullet for player one
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet.Y = GameInfo.PlayerOnePosition.Y;
        GameInfo.PlayerOneBullet.X = GameInfo.PlayerOnePosition.X + 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        Draw(GameInfo);
    }
    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }
            
            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerTwoBullet.Y = 0;
        GameInfo.PlayerTwoBullet.X = 79;
        Draw(GameInfo);
    }
    return 0;
}

int Wait()
{   
    INPUT_RECORD InputRecord;
    DWORD Events = 0;
    
    if(WAIT_TIMEOUT == WaitForSingleObject(hInput,1))
                return 0;
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    if(InputRecord.EventType == KEY_EVENT){
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 1;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 2;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_O && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 3;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_L && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 4;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 5;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_K && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 6;
    }
    FlushConsoleInputBuffer(hInput);
    return 0;
}

void LaunchBullet2(GAMEINFO &GameInfo, int PlayerNumber)
{
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet2.X = GameInfo.PlayerOnePosition.X + 1;
        GameInfo.PlayerOneBullet2.Y = GameInfo.PlayerOnePosition.Y;

        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
            if(GameInfo.PlayerOneBullet2.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet2.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }

    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet2.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet2.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
            if(GameInfo.PlayerTwoBullet2.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet2.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }
}

void UpdateScore(int player)
{
    // Update score based on player number
    if(player == 1)
        scorePlayerOne++;
    else if(player == 2)
        scorePlayerTwo++;

    // Display updated scores
    COORD scorePos = {0, 1}; // Position for score display
    SetConsoleCursorPosition(hOutput, scorePos);
    cout << "Player 1 Score: " << scorePlayerOne << " | Player 2 Score: " << scorePlayerTwo;
}

void AIBehavior(GAMEINFO &GameInfo)
{
    // Simple AI for player two
    if(isAIEnabled){
        // Randomly decide to move up or down
        int decision = rand() % 2;
        if(decision == 0 && GameInfo.PlayerTwoPosition.Y > 0){ // Move up
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y--;
            Draw(GameInfo);
        } else if(GameInfo.PlayerTwoPosition.Y < 24){ // Move down
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y++;
            Draw(GameInfo);
        }
    }
}

void CheckBulletCollision(GAMEINFO &GameInfo)
{
    // Check if player one's bullet hits player two
    if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X && GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
        UpdateScore(1); // Player one scores
        GameInfo.PlayerOneBullet.Y = 0; // Reset bullet position
    }
    // Check if player two's bullet hits player one
    if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X && GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
        UpdateScore(2); // Player two scores
        GameInfo.PlayerTwoBullet.Y = 0; // Reset bullet position
    }
}

void Movement(GAMEINFO &GameInfo)
{
    INPUT_RECORD InputRecord;
    DWORD Events = 0;

    // Read input from the console
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    // Process keyboard events
    if(InputRecord.EventType == KEY_EVENT){
        // Player 1 movement and shooting
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y--;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y++;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown){
            LaunchBullet(GameInfo, 1);
        }

        // Player 2 movement and shooting (handled by AI if enabled)
        AIBehavior(GameInfo);

        // Check for bullet collisions
        CheckBulletCollision(GameInfo);
    }
}

void Draw(GAMEINFO GameInfo)
{
    // Draw player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << "|";
    // Draw player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << "|";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << ".";
    
    GameInfo.ZeroZero.X = 0;
    GameInfo.ZeroZero.Y = 0;

    int i;
    for(i = 0; i < 79; i++){
        SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
        cout << ".";
        GameInfo.ZeroZero.X++;
    }

}

void Erase(GAMEINFO GameInfo)
{
    // Erase player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << " ";
    // Erase player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << " ";
}

int LaunchBullet(GAMEINFO &GameInfo, int PlayerNumber)
{   

    int i;
    // Launch bullet for player one
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet.Y = GameInfo.PlayerOnePosition.Y;
        GameInfo.PlayerOneBullet.X = GameInfo.PlayerOnePosition.X + 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        Draw(GameInfo);
    }
    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }
            
            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerTwoBullet.Y = 0;
        GameInfo.PlayerTwoBullet.X = 79;
        Draw(GameInfo);
    }
    return 0;
}

int Wait()
{   
    INPUT_RECORD InputRecord;
    DWORD Events = 0;
    
    if(WAIT_TIMEOUT == WaitForSingleObject(hInput,1))
                return 0;
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    if(InputRecord.EventType == KEY_EVENT){
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 1;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 2;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_O && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 3;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_L && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 4;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 5;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_K && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 6;
    }
    FlushConsoleInputBuffer(hInput);
    return 0;
}

void LaunchBullet2(GAMEINFO &GameInfo, int PlayerNumber)
{
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet2.X = GameInfo.PlayerOnePosition.X + 1;
        GameInfo.PlayerOneBullet2.Y = GameInfo.PlayerOnePosition.Y;

        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
            if(GameInfo.PlayerOneBullet2.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet2.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }

    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet2.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet2.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
            if(GameInfo.PlayerTwoBullet2.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet2.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }
}

void UpdateScore(int player)
{
    // Update score based on player number
    if(player == 1)
        scorePlayerOne++;
    else if(player == 2)
        scorePlayerTwo++;

    // Display updated scores
    COORD scorePos = {0, 1}; // Position for score display
    SetConsoleCursorPosition(hOutput, scorePos);
    cout << "Player 1 Score: " << scorePlayerOne << " | Player 2 Score: " << scorePlayerTwo;
}

void AIBehavior(GAMEINFO &GameInfo)
{
    // Simple AI for player two
    if(isAIEnabled){
        // Randomly decide to move up or down
        int decision = rand() % 2;
        if(decision == 0 && GameInfo.PlayerTwoPosition.Y > 0){ // Move up
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y--;
            Draw(GameInfo);
        } else if(GameInfo.PlayerTwoPosition.Y < 24){ // Move down
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y++;
            Draw(GameInfo);
        }
    }
}

void CheckBulletCollision(GAMEINFO &GameInfo)
{
    // Check if player one's bullet hits player two
    if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X && GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
        UpdateScore(1); // Player one scores
        GameInfo.PlayerOneBullet.Y = 0; // Reset bullet position
    }
    // Check if player two's bullet hits player one
    if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X && GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
        UpdateScore(2); // Player two scores
        GameInfo.PlayerTwoBullet.Y = 0; // Reset bullet position
    }
}
void Movement(GAMEINFO &GameInfo)
{
    INPUT_RECORD InputRecord;
    DWORD Events = 0;

    // Read input from the console
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    // Process keyboard events
    if(InputRecord.EventType == KEY_EVENT){
        // Player 1 movement and shooting
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y--;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y++;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown){
            FireWeapon(GameInfo, 1);
        }

        // Player 2 movement and shooting (handled by AI if enabled)
        AIBehavior(GameInfo);

        // Check for bullet collisions
        CheckBulletCollision(GameInfo);
    }
}

void Draw(GAMEINFO GameInfo)
{
    // Draw player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << "|";
    // Draw player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << "|";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << ".";
    
    GameInfo.ZeroZero.X = 0;
    GameInfo.ZeroZero.Y = 0;

    int i;
    for(i = 0; i < 79; i++){
        SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
        cout << ".";
        GameInfo.ZeroZero.X++;
    }

    // Draw all bullets
    for (auto &bullet : GameInfo.PlayerOneBullets) {
        SetConsoleCursorPosition(hOutput, bullet);
        cout << '*';
    }
    for (auto &bullet : GameInfo.PlayerTwoBullets) {
        SetConsoleCursorPosition(hOutput, bullet);
        cout << '*';
    }

    // Draw all power-ups
    for (auto &powerUp : GameInfo.PowerUps) {
        SetConsoleCursorPosition(hOutput, powerUp);
        cout << '$'; // Display power-up as $
    }

    // Draw obstacles
    for (auto &obstacle : GameInfo.Obstacles) {
        SetConsoleCursorPosition(hOutput, obstacle);
        cout << '#'; // Display obstacles as #
    }
}

void Erase(GAMEINFO GameInfo)
{
    // Erase player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << " ";
    // Erase player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << " ";
}

int LaunchBullet(GAMEINFO &GameInfo, int PlayerNumber)
{   

    int i;
    // Launch bullet for player one
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet.Y = GameInfo.PlayerOnePosition.Y;
        GameInfo.PlayerOneBullet.X = GameInfo.PlayerOnePosition.X + 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        Draw(GameInfo);
    }
    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            case 5:
                LaunchBullet2(GameInfo, 1);
                return 0;
                break;
            case 6:
                LaunchBullet2(GameInfo, 2);
                return 0;
                break;
            }
            
            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerTwoBullet.Y = 0;
        GameInfo.PlayerTwoBullet.X = 79;
        Draw(GameInfo);
    }
    return 0;
}

int Wait()
{   
    INPUT_RECORD InputRecord;
    DWORD Events = 0;
    
    if(WAIT_TIMEOUT == WaitForSingleObject(hInput,1))
                return 0;
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    if(InputRecord.EventType == KEY_EVENT){
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 1;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 2;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_O && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 3;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_L && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 4;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 5;
        
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_K && InputRecord.Event.KeyEvent.bKeyDown == 1)
            return 6;
    }
    FlushConsoleInputBuffer(hInput);
    return 0;
}

void LaunchBullet2(GAMEINFO &GameInfo, int PlayerNumber)
{
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet2.X = GameInfo.PlayerOnePosition.X + 1;
        GameInfo.PlayerOneBullet2.Y = GameInfo.PlayerOnePosition.Y;

        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
            if(GameInfo.PlayerOneBullet2.X == GameInfo.PlayerTwoPosition.X)
                if(GameInfo.PlayerOneBullet2.Y == GameInfo.PlayerTwoPosition.Y){
                    system("cls");
                    cout << "\aPlayer 1 Wins" << endl;
                    system("pause");
                    exit(0);
                }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }

    if(PlayerNumber == 2){
        GameInfo.PlayerTwoBullet2.Y = GameInfo.PlayerTwoPosition.Y;
        GameInfo.PlayerTwoBullet2.X = GameInfo.PlayerTwoPosition.X - 1;
        Draw(GameInfo);
        Erase(GameInfo);
        int i;
        for(i = 0; i < 77; i++){
            GameInfo.PlayerTwoBullet.X -= 1;
            GameInfo.PlayerTwoBullet2.X -= 1;
            GameInfo.PlayerOneBullet.X += 1;
            GameInfo.PlayerOneBullet2.X += 1;
            Draw(GameInfo);
        
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break;
            case 3:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y--;
                if(GameInfo.PlayerTwoPosition.Y < 0)
                    GameInfo.PlayerTwoPosition.Y++;
                break;
            case 4:
                Erase(GameInfo);
                GameInfo.PlayerTwoPosition.Y++;
                if(GameInfo.PlayerTwoPosition.Y > 24)
                    GameInfo.PlayerTwoPosition.Y--;
                break;
            }

            Draw(GameInfo);
            Erase(GameInfo);
            if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
            if(GameInfo.PlayerTwoBullet2.X == GameInfo.PlayerOnePosition.X)
                if(GameInfo.PlayerTwoBullet2.Y == GameInfo.PlayerOnePosition.Y){
                    system("cls");
                    cout << "\aPlayer 2 Wins" << endl;
                    system("pause");
                    exit(0);
            }
        }
        GameInfo.PlayerOneBullet.Y = 0;
        GameInfo.PlayerOneBullet.X = 0;
        GameInfo.PlayerOneBullet2.Y = 0;
        GameInfo.PlayerOneBullet2.X = 1;
        Draw(GameInfo);
    }
}

void UpdateScore(int player)
{
    // Update score based on player number
    if(player == 1)
        scorePlayerOne++;
    else if(player == 2)
        scorePlayerTwo++;

    // Display updated scores
    COORD scorePos = {0, 1}; // Position for score display
    SetConsoleCursorPosition(hOutput, scorePos);
    cout << "Player 1 Score: " << scorePlayerOne << " | Player 2 Score: " << scorePlayerTwo;
}

void AIBehavior(GAMEINFO &GameInfo)
{
    // Simple AI for player two
    if(isAIEnabled){
        // Randomly decide to move up or down
        int decision = rand() % 2;
        if(decision == 0 && GameInfo.PlayerTwoPosition.Y > 0){ // Move up
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y--;
            Draw(GameInfo);
        } else if(GameInfo.PlayerTwoPosition.Y < 24){ // Move down
            Erase(GameInfo);
            GameInfo.PlayerTwoPosition.Y++;
            Draw(GameInfo);
        }
    }
}

void CheckBulletCollision(GAMEINFO &GameInfo)
{
    // Check if player one's bullet hits player two
    if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X && GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
        UpdateScore(1); // Player one scores
        GameInfo.PlayerOneBullet.Y = 0; // Reset bullet position
    }
    // Check if player two's bullet hits player one
    if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X && GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
        UpdateScore(2); // Player two scores
        GameInfo.PlayerTwoBullet.Y = 0; // Reset bullet position
    }
}

void Movement(GAMEINFO &GameInfo)
{
    INPUT_RECORD InputRecord;
    DWORD Events = 0;

    // Read input from the console
    ReadConsoleInput(hInput, &InputRecord, 1, &Events);

    // Process keyboard events
    if(InputRecord.EventType == KEY_EVENT){
        // Player 1 movement and shooting
        if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_Q && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y--;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_A && InputRecord.Event.KeyEvent.bKeyDown){
            Erase(GameInfo);
            GameInfo.PlayerOnePosition.Y++;
            Draw(GameInfo);
        } else if(InputRecord.Event.KeyEvent.wVirtualKeyCode == VK_S && InputRecord.Event.KeyEvent.bKeyDown){
            LaunchBullet(GameInfo, 1);
        }

        // Player 2 movement and shooting (handled by AI if enabled)
        AIBehavior(GameInfo);

        // Check for bullet collisions
        CheckBulletCollision(GameInfo);
    }
}

void Draw(GAMEINFO GameInfo)
{
    // Draw player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << "|";
    // Draw player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << "|";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << ".";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << ".";
    
    GameInfo.ZeroZero.X = 0;
    GameInfo.ZeroZero.Y = 0;

    int i;
    for(i = 0; i < 79; i++){
        SetConsoleCursorPosition(hOutput, GameInfo.ZeroZero);
        cout << ".";
        GameInfo.ZeroZero.X++;
    }

}

void Erase(GAMEINFO GameInfo)
{
    // Erase player one
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOnePosition);
    cout << " ";
    // Erase player two
    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoPosition);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerOneBullet2);
    cout << " ";

    SetConsoleCursorPosition(hOutput, GameInfo.PlayerTwoBullet2);
    cout << " ";
}

int LaunchBullet(GAMEINFO &GameInfo, int PlayerNumber)
{   

    int i;
    // Launch bullet for player one
    if(PlayerNumber == 1){
        GameInfo.PlayerOneBullet.Y = GameInfo.PlayerOnePosition.Y;
        GameInfo.PlayerOneBullet.X = GameInfo.PlayerOnePosition.X + 1;
        Draw(GameInfo);
        Erase(GameInfo);
        for(i = 0; i < 77; i++){
            GameInfo.PlayerOneBullet.X += 1;
            Draw(GameInfo);
    
            int move;
            move =    Wait();
            switch(move){
            case 1:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y--;
                if(GameInfo.PlayerOnePosition.Y < 0)
                    GameInfo.PlayerOnePosition.Y++;
                break;
            case 2:
                Erase(GameInfo);
                GameInfo.PlayerOnePosition.Y++;
                if(GameInfo.PlayerOnePosition.Y > 24)
                    GameInfo.PlayerOnePosition.Y--;
                break Draw(GameInfo);
        }
    }
}

void CheckBulletCollision(GAMEINFO &GameInfo)
{
    // Check if player one's bullet hits player two
    if(GameInfo.PlayerOneBullet.X == GameInfo.PlayerTwoPosition.X && GameInfo.PlayerOneBullet.Y == GameInfo.PlayerTwoPosition.Y){
        UpdateScore(1); // Player one scores
        GameInfo.PlayerOneBullet.Y = 0; // Reset bullet position
    }
    // Check if player two's bullet hits player one
    if(GameInfo.PlayerTwoBullet.X == GameInfo.PlayerOnePosition.X && GameInfo.PlayerTwoBullet.Y == GameInfo.PlayerOnePosition.Y){
        UpdateScore(2); // Player two scores
        GameInfo.PlayerTwoBullet.Y = 0; // Reset bullet position
    }
}
