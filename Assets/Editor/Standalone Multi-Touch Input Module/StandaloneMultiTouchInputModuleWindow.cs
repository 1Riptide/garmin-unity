/* Written by Kaz Crowe */
/* StandaloneMultiTouchInputModuleWindow.cs ver 1.0 */
using UnityEngine;
using UnityEditor;

public class StandaloneMultiTouchInputModuleWindow : EditorWindow
{
	GUILayoutOption[] buttonSize = new GUILayoutOption[] { GUILayout.Width( 200 ), GUILayout.Height( 35 ) };
	GUILayoutOption[] buttonSizeLarge = new GUILayoutOption[] { GUILayout.Width( 200 ), GUILayout.Height( 75 ) }; 
	
	GUILayoutOption[] docSize = new GUILayoutOption[] { GUILayout.Width( 300 ), GUILayout.Height( 330 ) };
	
	GUISkin style;
	
	enum CurrentMenu
	{
		MainMenu,
		HowTo,
		OtherProducts,
		Feedback,
		ThankYou
	}
	static CurrentMenu currentMenu;
	static string menuTitle = "Main Menu";

	Texture2D howToAdd01, howToAdd02, howToAdd03, howToAdd04;

	Texture2D ujPromo, ubPromo, usbPromo, fstpPromo;
	
	Vector2 scroll_HowTo = Vector2.zero, scroll_OtherProd = Vector2.zero;
	Vector2 scroll_Feedback = Vector2.zero, scroll_Thanks = Vector2.zero;
	
	[MenuItem( "Window/Ultimate UI/Standalone Multi-Touch Input Module", false, 0 )]
	static void Init ()
	{
		InitializeWindow();
	}

	static void InitializeWindow ()
	{
		EditorWindow window = ( StandaloneMultiTouchInputModuleWindow )GetWindow<StandaloneMultiTouchInputModuleWindow>( true, "Tank and Healer Studio Asset Window", true );
		window.maxSize = new Vector2( 500, 500 );
		window.minSize = new Vector2( 500, 500 );
		window.Show();
	}
	
	void OnEnable ()
	{
		style = ( GUISkin )EditorGUIUtility.Load( "Standalone Multi-Touch Input Module/StandaloneMultiTouchInputModuleSkin.guiskin" );

		howToAdd01 = ( Texture2D )EditorGUIUtility.Load( "Standalone Multi-Touch Input Module/HowToAdd01.jpg" );
		howToAdd02 = ( Texture2D )EditorGUIUtility.Load( "Standalone Multi-Touch Input Module/HowToAdd02.jpg" );
		howToAdd03 = ( Texture2D )EditorGUIUtility.Load( "Standalone Multi-Touch Input Module/HowToAdd03.jpg" );
		howToAdd04 = ( Texture2D )EditorGUIUtility.Load( "Standalone Multi-Touch Input Module/HowToAdd04.jpg" );
		
		ujPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/UJ_Promo.png" );
		ubPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/UB_Promo.png" );
		usbPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/USB_Promo.png" );
		fstpPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/FSTP_Promo.png" );
	}
	
	void OnGUI ()
	{
		if( style == null )
		{
			GUILayout.BeginVertical( "Box" );
			GUILayout.FlexibleSpace();
			
			ErrorScreen();
			
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			return;
		}
		
		GUI.skin = style;
		
		EditorGUILayout.Space();
		
		GUILayout.BeginVertical( "Box" );
		
		EditorGUILayout.LabelField( "S.M.T.I.M.", GUI.skin.GetStyle( "WindowTitle" ) );
		
		GUILayout.Space( 3 );
		
		EditorGUILayout.LabelField( " Version 2.0.2", EditorStyles.whiteMiniLabel );//< ---- ALWAYS UPDATE
		
		GUILayout.Space( 12 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 5 );
		if( currentMenu != CurrentMenu.MainMenu && currentMenu != CurrentMenu.ThankYou )
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space( 5 );
			if( GUILayout.Button( "", GUI.skin.GetStyle( "BackButton" ), GUILayout.Width( 80 ), GUILayout.Height( 40 ) ) )
				BackToMainMenu();
			EditorGUILayout.EndVertical();
		}
		else
			GUILayout.Space( 80 );
		
		GUILayout.Space( 15 );
		EditorGUILayout.BeginVertical();
		GUILayout.Space( 14 );
		EditorGUILayout.LabelField( menuTitle, GUI.skin.GetStyle( "HeaderText" ) );
		EditorGUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 80 );
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		switch( currentMenu )
		{
		case CurrentMenu.MainMenu:
		{
			MainMenu();
		}break;
		case CurrentMenu.HowTo:
		{
			HowTo();
		}break;
		case CurrentMenu.OtherProducts:
		{
			OtherProducts();
		}break;
		case CurrentMenu.Feedback:
		{
			Feedback();
		}break;
		case CurrentMenu.ThankYou:
		{
			ThankYou();
		}break;
		default:
		{
			MainMenu();
		}break;
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Space( 20 );
		EditorGUILayout.EndVertical();
	}
	
	void ErrorScreen ()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 50 );
		EditorGUILayout.LabelField( "ERROR", EditorStyles.boldLabel );
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 50 );
		EditorGUILayout.LabelField( "Could not find the needed GUISkin located in the Editor Default Resources folder. Please ensure that the correct GUISkin, StandaloneMultiTouchInputModuleSkin, is in the right folder( Editor Default Resources/Standalone Multi-Touch Input Module ) before trying to access the Standalone Multi-Touch Input Module Window.", EditorStyles.wordWrappedLabel );
		GUILayout.Space( 50 );
		EditorGUILayout.EndHorizontal();
	}
	
	void BackToMainMenu ()
	{
		currentMenu = CurrentMenu.MainMenu;
		menuTitle = "Main Menu";
	}
	
	#region MainMenu
	void MainMenu ()
	{
		EditorGUILayout.BeginVertical();
		GUILayout.Space( 25 );

		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "How To", buttonSizeLarge ) )
		{
			currentMenu = CurrentMenu.HowTo;
			menuTitle = "How To";
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Other Products", buttonSizeLarge ) )
		{
			currentMenu = CurrentMenu.OtherProducts;
			menuTitle = "Other Products";
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Feedback", buttonSizeLarge ) )
		{
			currentMenu = CurrentMenu.Feedback;
			menuTitle = "Feedback";
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
	}
	#endregion
	
	#region HowTo
	void HowTo ()
	{
		scroll_HowTo = EditorGUILayout.BeginScrollView( scroll_HowTo, false, false, docSize );
		
		EditorGUILayout.LabelField( "How To Add", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   To add the Standalone Multi-Touch Input Module to your EventSystem, first select your EventSystem in the Hierarchy window.", EditorStyles.wordWrappedLabel );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( howToAdd01 );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Then select the Add Component menu and find the Event section.", EditorStyles.wordWrappedLabel );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( howToAdd02 );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Then find and select the Standalone Multi-Touch Input Module option.", EditorStyles.wordWrappedLabel );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( howToAdd03 );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "This will add the Standalone Multi-Touch Input Module to your selected EventSystem.", EditorStyles.wordWrappedLabel );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( howToAdd04 );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "The script will disable the Standalone Input Module as well as the Touch Input Module in order to work correctly. Now you are free to test your game on a mobile device and use multi-touch functionality.", EditorStyles.wordWrappedLabel );

		GUILayout.FlexibleSpace();
		
		EditorGUILayout.EndScrollView();
	}
	#endregion

	#region OtherProducts
	void OtherProducts ()
	{
		scroll_OtherProd = EditorGUILayout.BeginScrollView( scroll_OtherProd, false, false, docSize );

		/* -------------- < ULTIMATE JOYSTICK > -------------- */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( ujPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Ultimate Joystick", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   The Ultimate Joystick is a simple, yet powerful tool for the development of your mobile games. The Ultimate Joystick was created with the goal of giving Developers an incredibly versatile joystick solution, while being extremely easy to implement into existing, or new scripts. You don't need to be a programmer to work with the Ultimate Joystick, and it is very easy to implement into any type of character controller that you need. Additionally, Ultimate Joystick's source code is extremely well commented, easy to modify, and features a complete in-engine documentation window, making it ideal for game-specific adjustments. In its entirety, Ultimate Joystick is an elegant and easy to utilize mobile joystick solution.", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/ultimate-joystick.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* ------------ < END ULTIMATE JOYSTICK > ------------ */
		
		GUILayout.Space( 25 );

		/* -------------- < ULTIMATE BUTTON > -------------- */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( ubPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Ultimate Button", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   Buttons are a core element of UI, and as such they should be easy to customize and implement. The Ultimate Button is the embodiment of that very idea. This code package takes the best of Unity's Input and UnityEvent methods and pairs it with exceptional customization to give you the most versatile button for your mobile project. Are you in need of a button for attacking, jumping, shooting, or all of the above? With Ultimate Button's easy size and placement options, style options, and touch actions, you'll have everything you need to create your custom buttons, whether they are simple or complex.", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/ultimate-button.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* ------------ < END ULTIMATE BUTTON > ------------ */
		
		GUILayout.Space( 25 );
		
		/* ------------ < ULTIMATE STATUS BAR > ------------ */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( usbPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Ultimate Status Bar", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   The Ultimate Status Bar is a complete solution for displaying health, mana, energy, stamina, experience, or virtually any condition that you'd like like your player to be aware of. It can also be used to show a selected target's health, the progress of loading or casting, and even interacting with objects. Whatever type of progress display that you need, the Ultimate Status Bar can make it visually happen. Additionally, you have the option of using the many \"Ultimate\" textures provided, or you can easily use your own! If you are looking for a way to neatly display any type of status for your game, then consider the Ultimate Status Bar!", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/ultimate-status-bar.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* -------------- < END STATUS BAR > --------------- */
		
		GUILayout.Space( 25 );
		
		/* -------------- < FROST STONE > -------------- */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( fstpPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Frost Stone: UI Texture Pack", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   This package is made to compliment Ultimate Joystick, Ultimate Button and Ultimate Status Bar. The Frost Stone: UI Texture Pack is an inspiring new look for your Ultimate Joystick, Ultimate Button and Ultimate Status Bar. These Frost Stone Textures will flawlessly blend with your current Ultimate UI code to give your game an incredible new look.", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/frost-stone-texture-pack.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* ------------ < END FROST STONE > ------------ */
		
		EditorGUILayout.EndScrollView();
	}
	#endregion
	
	#region Feedback
	void Feedback ()
	{
		scroll_Feedback = EditorGUILayout.BeginScrollView( scroll_Feedback, false, false, docSize );
		
		EditorGUILayout.LabelField( "Having Problems?", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   If you experience any issues with the Standalone Multi-Touch Input Module, please email us right away. We will lend any assistance that we can to resolve any issues that you have.", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Support Email:\n    tankandhealerstudio@outlook.com" , EditorStyles.boldLabel, GUILayout.Height( 30 ) );
		
		GUILayout.Space( 25 );
		
		EditorGUILayout.LabelField( "Good Experiences?", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   If you have appreciated how easy the Standalone Multi-Touch Input Module is to use in your project, leave us a comment and rating on the Unity Asset Store. We are very grateful for all positive feedback that we get.", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Rate Us", buttonSize ) )
			Application.OpenURL( "https://www.assetstore.unity3d.com/en/#!/content/28861" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 25 );
		
		EditorGUILayout.LabelField( "Show Us What You've Done!", GUI.skin.GetStyle( "SectionHeader" ) );
		
		EditorGUILayout.LabelField( "   If you have used any of the assets created by Tank & Healer Studio in your project, we would love to see what you have done. Contact us with any information on your game and we will be happy to support you in any way that we can!", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 5 );
		
		EditorGUILayout.LabelField( "Contact Us:\n    tankandhealerstudio@outlook.com" , EditorStyles.boldLabel, GUILayout.Height( 30 ) );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( "Happy Game Making,\n	-Tank & Healer Studio", GUILayout.Height( 30 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 25 );
		
		EditorGUILayout.EndScrollView();
	}
	#endregion
	
	#region ThankYou
	void ThankYou ()
	{
		scroll_Thanks = EditorGUILayout.BeginScrollView( scroll_Thanks, false, false, docSize );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.LabelField( "    The two of us at Tank & Healer Studio would like to thank you for downloading the Standalone Multi-Touch Input Module asset package from the Unity Asset Store. If you have any questions about this asset that are not covered in this Documentation Window, please don't hesitate to contact us at: ", EditorStyles.wordWrappedLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.LabelField( "       tankandhealerstudio@outlook.com" , EditorStyles.boldLabel );
		
		GUILayout.Space( 10 );
		
		EditorGUILayout.LabelField( "    We hope that the Standalone Multi-Touch Input Module will be a great help to you in the development of your game. After pressing the continue button below, you will be presented with helpful information on this asset to assist you in getting this asset working in your project.\n", EditorStyles.wordWrappedLabel );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( "Happy Game Making,\n	-Tank & Healer Studio", GUILayout.Height( 30 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space( 15 );
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Continue", buttonSize ) )
		{
			EditorPrefs.SetBool( "StandaloneMultiTouchInputModuleStartup", true );
			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( "Assets/Plugins/Standalone Multi-Touch Input Module/README.txt" );
			BackToMainMenu();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndScrollView();
	}
	#endregion
	
	[InitializeOnLoad]
	class StandaloneMultiTouchInputModuleInitialLoad
	{
		static StandaloneMultiTouchInputModuleInitialLoad ()
		{
			if( EditorPrefs.GetBool( "StandaloneMultiTouchInputModuleStartup" ) == false )
				EditorApplication.update += WaitForCompile;
		}
		
		static void WaitForCompile ()
		{
			if( EditorApplication.isCompiling )
				return;
			
			EditorApplication.update -= WaitForCompile;
			
			currentMenu = CurrentMenu.ThankYou;
			menuTitle = "Thank You";
			
			InitializeWindow();

			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( "Assets/Plugins/Standalone Multi-Touch Input Module/README.txt" );
		}
	}
}