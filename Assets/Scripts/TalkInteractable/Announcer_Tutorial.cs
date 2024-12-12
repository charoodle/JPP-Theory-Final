using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Avoid using Unity.CharacterController
using CharacterController = MyProject.CharacterController;

/// <summary>
/// Janky workaround to not make a separate banner-type dialogue manager to announce the beginning of a game.
/// 
/// Can just call Talk_BeginGame()
/// </summary>
public class Announcer_Tutorial : TalkWithInteractable
{
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerTrigger jumpFenceTrigger;
    [SerializeField] PlayerTrigger startShootingAreaTutorial;
    [SerializeField] PlayerTrigger startTrebuchetAreaTutorial;

    // Ability to remove it if needed.
    [SerializeField] GameObject fence;

    // Remove when player finish shooting tutorial.
    [SerializeField] GameObject gateToTrebuchet;

    // Trebuchet + buttons
    [SerializeField] Trebuchet trebuchet;
    [SerializeField] Interactable trebuchetLaunchButton;
    [SerializeField] GameObject trebuchetReloadButton;
    [SerializeField] GameObject trebuchetWeightButton0;
    [SerializeField] GameObject trebuchetWeightButton1;
    [SerializeField] GameObject trebuchetTurnButton0;
    [SerializeField] GameObject trebuchetTurnButton1;



#if UNITY_EDITOR
    [SerializeField] bool beginGame;
#endif

    private void Awake()
    {
        // Should not be interactable by the player.
        interactText = "NOT PLAYER-INTERACTABLE.";
        playerCanInteractWith = false;
    }

    private void OnEnable()
    {
        startShootingAreaTutorial.OnPlayerEnter += BeginTalk_Shooting;
        startTrebuchetAreaTutorial.OnPlayerEnter += BeginTalk_Trebuchet;
    }

    private void OnDisable()
    {
        startShootingAreaTutorial.OnPlayerEnter -= BeginTalk_Shooting;
    }

    protected override void Start()
    {
        base.Start();

        // Prevent all player controls, let tutorials enable one by one
        DisableAllPlayerControls();
        // Start tutorial on player load in
        StartCoroutine(GameStartBeginTalk());

        // Trebuchet Tutorial - Disable all trebuchet buttons except launch button
        trebuchetLaunchButton.playerCanInteractWith = false;        // Holds trebuchet rigidbody down physically, so can't just set inactive
        trebuchetTurnButton0.SetActive(false);
        trebuchetTurnButton1.SetActive(false);
        trebuchetWeightButton0.SetActive(false);
        trebuchetWeightButton1.SetActive(false);
    }

    protected IEnumerator GameStartBeginTalk()
    {
        // Slight delay
        yield return new WaitForSeconds(2f);

        BeginTalk_Movement();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && beginGame)
        {
            // Don't begin again if running
            if (isRunning)
                return;
            // Start talk
            BeginTalk_Movement();
            // Turn off bool in inspector
            beginGame = false;
        }
    }
#endif

    /// <summary>
    /// Since no character, so just circumvent my poorly designed system and call the coroutine with a more verbose function name.
    /// </summary>
    public void BeginTalk_Movement()
    {
        StartCoroutine(TalkWithCoroutine());
    }

    /// <summary>
    /// Start the shooting tutorial.
    /// </summary>
    public void BeginTalk_Shooting()
    {
        // Event fired once, don't need anymore
        startShootingAreaTutorial.OnPlayerEnter -= BeginTalk_Shooting;
        startShootingAreaTutorial.Enable(false);

        StartCoroutine(TalkWithCoroutine_Shooting());
    }

    public void BeginTalk_Trebuchet()
    {
        // Event fired once, don't need anymore
        startTrebuchetAreaTutorial.OnPlayerEnter -= BeginTalk_Trebuchet;
        startTrebuchetAreaTutorial.Enable(false);

        StartCoroutine(TalkWithCoroutine_Trebuchet());
    }

    protected void DisableAllPlayerControls()
    {
        // Disable player controls initially, tutorial will enable them one by one
        playerController.canInputLook = false;
        playerController.canInputMove = false;
        playerController.canInputJump = false;
        playerController.canInputSprint = false;
        // Disable player weapon controls; will be unlocked in the shooting tutorial
        playerController.canSwitchToPistol = false;
        playerController.canSwitchToRocketLauncher = false;
        //playerController.canSwitchToUnarmed = false;  // Faster movespeed tutorial/function not implemented yet
    }

    protected override IEnumerator TalkWithCoroutine()
    {
        yield return StartTalk();

        DisableAllPlayerControls();

        yield return TextBox("Voice", $"Welcome! You can use {advanceTextKey.ToString()} to advance text, if you see a little black square in the bottom right.");

        yield return TextBox("This is your first time here, right? Let me teach you the basics.");
        // Longer wait time so player takes time to not skip through carelessly.
        yield return TextBox("I will not repeat myself, so listen up very carefully!", minAppearTime: 2.5f);
        yield return TextBox("You can use [W A S D] to move around, and your [Mouse] to look around.", waitCondition: WaitUntilCharacterMovesAndLooksAround(playerController));
        // Player can run if they want to bug out tutorial
        playerController.canInputSprint = true;
        yield return TextBox("Press [SPACEBAR] to jump.", waitCondition: WaitUntilCharacterJumpsOverFence(playerController, jumpFenceTrigger));
        yield return TextBox("Hold [LEFT SHIFT] to run.", waitCondition: WaitUntilCharacterRuns(playerController));
        yield return TextBox("Fairly standard FPS shooter controls.");
        yield return TextBox("Follow the path to the shooting range.");

        //yield return TextBox("Use the top-row number keys 1-2-3 to switch your weapons between pistol, rocket launcher, and unarmed.");
        //yield return TextBox("You will move faster and jump higher when you're unarmed.");

        yield return EndTalk();
    }

    protected IEnumerator TalkWithCoroutine_Shooting()
    {
        playerController.canSwitchToPistol = false;
        playerController.canSwitchToRocketLauncher = false;
        playerController.canSwitchToUnarmed = false;

        const float secondsToAdmireGun = 2.5f;

        yield return StartTalk();

        yield return TextBox("Voice", "It's time to teach you about weapons.");

        yield return TextBox("Press [1] to switch to your pistol.", waitCondition: WaitUntilPlayerPullsPistolOut(playerController));

        yield return new WaitForSeconds(secondsToAdmireGun);

        // Longer appear time if player fires weapon early; don't want them to skip box on accident
        playerController.canFireWeaponInHand = false;
        yield return TextBox("Now press Mouse0 to fire your weapon in hand. Aim a little high, because the projectiles will always be pulled down by gravity.", minAppearTime: 2f);
        {
            playerController.canFireWeaponInHand = true;
        }
        yield return WaitUntilPlayerFiresCurrentWeapon(playerController);

        
        yield return new WaitForSeconds(secondsToAdmireGun);

        yield return TextBox("Keep firing until its empty. It will automatically start reloading by itself, at the exact moment it's empty of bullets. There is no button to manually reload.", waitCondition: WaitUntilPlayersGunReloads(playerController));

        // Prevent pistol from reloading fully, so player switches to rocket launcher
        ProjectileLauncher pistol = playerController.GetCurrentWeapon();
        {
            pistol.preventReloadFromFinishing = true;
            // Force player to switch to rocket launcher
            playerController.canSwitchToPistol = false;
        }

        
        yield return new WaitForSeconds(secondsToAdmireGun);

        yield return TextBox("Now while that's automatically reloading, press [2] to switch to your rocket launcher.", waitCondition: WaitUntilPlayerPullsRocketLauncherOut(playerController));

        {
            playerController.canFireWeaponInHand = false;
        }
        yield return new WaitForSeconds(secondsToAdmireGun);
        yield return TextBox("Use Mouse0 to fire it. Make sure to point it upwards a little.");
        {
            playerController.canFireWeaponInHand = true;
        }

        yield return WaitUntilPlayerFiresCurrentWeapon(playerController);
        // Longer time if player accidentally trying out shooting rocket and skips box on accident
        yield return TextBox("This has an AOE on whatever it hits, it flies a little faster, but as a tradeoff it takes longer to reload.", minAppearTime: 1.5f);
        // Assumes rocket launcher is out
        ProjectileLauncher rocketLauncher = playerController.GetCurrentWeapon();
        {
            rocketLauncher.preventReloadFromFinishing = true;
        }

        yield return new WaitForSeconds(secondsToAdmireGun);
        yield return TextBox("In this land, you cannot manually reload weapons until they run out of their last bullet.");
        yield return TextBox("However, on the plus side, every weapon has infinite reserve ammo. So fire away to your heart's content.");
        {
            playerController.canSwitchToPistol = true;
        }

        yield return TextBox("Now switch back to your pistol using [1], it should be fully reloaded with ammo again.", waitCondition:WaitUntilPlayerPullsPistolOut(playerController));
        // Allow pistol to finish reload and display message
        {
            pistol.preventReloadFromFinishing = false;
            rocketLauncher.preventReloadFromFinishing = false;
            // Prevent player from firing 
            playerController.canFireWeaponInHand = false;
        }

        yield return new WaitForSeconds(secondsToAdmireGun);

        // Longer time in case player tried to fire pistol again after tut asked player to switch back to pistol
        yield return TextBox("Keep practicing your shots until you're comfortable enough with both weapons.", minAppearTime:2f);

        // Prevent player from looking & moving
        playerController.canInputLook = false;
        playerController.canInputMove = false;
        // Get gate positional info
        float origViewPitch = 0f;
        float origViewYaw = 0f;
        playerController.GetYawAndPitchDegrees(out origViewPitch, out origViewYaw);
        // Gate is at 0 on y, so look up from its pos a bit
        Vector3 gatePosition = gateToTrebuchet.transform.position + new Vector3(0f, 5f, 0f);
        float gatePitch = 0f;
        float gateYaw = 0f;
        playerController.GetTargetPitchAndYawFrom(gatePosition, out gateYaw, out gatePitch);
        // Make player look at gate
        playerController.LookAtTargetPitchYaw(gatePitch, gateYaw, withinDegrees:0.1f);
        // Make gate disappear
        gateToTrebuchet.SetActive(false);
        yield return TextBox("And when you're done, head on over to the next gate over. It's time to teach you how to use our ultimate weapon...", minAppearTime: 1f);

        // Make player look at trebuchet
        playerController.LookAt(trebuchet.transform);
        yield return TextBox("...the trebuchet.", minAppearTime: 2f);
        {
            playerController.canFireWeaponInHand = true;
        }

        // Wait until player returns to original look position
        yield return playerController.LookAtTargetPitchYawEnum(origViewPitch, origViewYaw, withinDegrees: 0.1f);

        // Enable player look & move input again when cutscene ends
        playerController.canInputLook = true;
        playerController.canInputMove = true;

        yield return EndTalk();

        #region Helper IEnumerators
        /// Waits until player switches to pistol
        IEnumerator WaitUntilPlayerPullsPistolOut(PlayerController player)
        {
            player.canSwitchToPistol = true;

            bool playerSwitchedToPistol = false;

            void OnPlayerSwitchWeapon(PlayerController.Weapon weapon)
            {
                if (weapon == PlayerController.Weapon.PISTOL)
                    playerSwitchedToPistol = true;
            }

            player.OnWeaponSwitch += OnPlayerSwitchWeapon;

            // Wait for player to switch to pistol
            while (!playerSwitchedToPistol)
            {
                yield return null;
            }

            player.OnWeaponSwitch -= OnPlayerSwitchWeapon;
        }

        /// Waits until any of player's guns starts reloading
        IEnumerator WaitUntilPlayersGunReloads(PlayerController player)
        {
            bool reloadStarted = false;
            void OnReloadStarted()
            {
                reloadStarted = true;
            }

            player.OnWeaponReloadStart += OnReloadStarted;

            // Wait for player start reloading weapon event
            while(!reloadStarted)
            {
                yield return null;
            }

            player.OnWeaponReloadStart -= OnReloadStarted;
        }

        IEnumerator WaitUntilPlayerFiresCurrentWeapon(PlayerController player)
        {
            bool weaponFired = false;
            void OnWeaponFired()
            {
                weaponFired = true;
            }

            player.OnWeaponLaunchProjectile += OnWeaponFired;

            // Wait for player to fire weapon event
            while (!weaponFired)
            {
                yield return null;
            }

            player.OnWeaponLaunchProjectile -= OnWeaponFired;
        }

        /// Waits until player switches to rocket launcher.
        IEnumerator WaitUntilPlayerPullsRocketLauncherOut(PlayerController player)
        {
            player.canSwitchToRocketLauncher = true;

            bool playerSwitchedToRocketLauncher = false;

            void OnPlayerSwitchWeapon(PlayerController.Weapon weapon)
            {
                if (weapon == PlayerController.Weapon.RL)
                    playerSwitchedToRocketLauncher = true;
            }

            player.OnWeaponSwitch += OnPlayerSwitchWeapon;

            // Wait for player to switch to rocket launcher
            while (!playerSwitchedToRocketLauncher)
            {
                yield return null;
            }

            player.OnWeaponSwitch -= OnPlayerSwitchWeapon;
        }
        #endregion
    }

    protected IEnumerator TalkWithCoroutine_Trebuchet()
    {
        #region Helper Enumerators
        IEnumerator WaitUntilPlayerPressesButton(GameObject buttonObjectThatCanBeSetActiveFalse)
        {
            // Wait until the becomes, thus launching the projectile
            while (buttonObjectThatCanBeSetActiveFalse.activeInHierarchy == true)
            {
                yield return null;
            }
        }

        IEnumerator WaitUntilRockLands(TrebuchetProjectile rock)
        {
            while(!rock.hasLanded)
            {
                yield return null;
            }
        }
        #endregion

        // All buttons should be disabled at first - they get disabled OnStart.

        // Prevent player from firing gun on accident
        playerController.canFireWeaponInHand = false;

        yield return StartTalk();

        yield return TextBox("Voice", "This is the ultimate siege weapon. The trebuchet.");

        yield return TextBox("It works by letting an extremely heavy counterweight drop, which can launch an extremely heavy projectile over long distances.");

        // Let player interact with button again
        trebuchetLaunchButton.playerCanInteractWith = true;

        // Wait until trebuchet launches
        yield return TextBox("To use it, go ahead and release the object that is restraining the long arm by pressing [E] on the white cube.", waitCondition:WaitUntilPlayerPressesButton(trebuchetLaunchButton.gameObject));
        // Make player look at trebuchet projectile when they press the button
        TrebuchetProjectile rock = FindObjectOfType<TrebuchetProjectile>();
        if (rock)
            playerController.LookAt(rock.transform, lookTime: 0.25f);

        // Freeze player controls
        playerController.canInputLook = false;
        playerController.canInputMove = false;
        playerController.canInputJump = false;

        // Make player view follow trebuchet ball and wait for it to land
        yield return WaitUntilRockLands(rock);
        // Gap to let particles play
        yield return new WaitForSeconds(1.5f);

        // Make player stop looking at rock
        playerController.LookAtStop();

        // Let player move again
        playerController.canInputLook = true;
        playerController.canInputMove = true;
        playerController.canInputJump = true;

        // WaitCond: Wait until player presses reload button on trebuchet
        yield return TextBox("In order to reload it, just press the white cube button that appears after the projectile is launched.", waitCondition:WaitUntilPlayerPressesButton(trebuchetReloadButton));

        yield return TextBox("It will automatically start and complete the reload process by itself.");

        yield return TextBox("When it's done reloading, all you need to do is release the cube on the long arm to launch it again.");

        // Reenable the rest of the button functionality of trebuchet
        trebuchetTurnButton0.SetActive(true);
        trebuchetTurnButton1.SetActive(true);
        trebuchetWeightButton0.SetActive(true);
        trebuchetWeightButton1.SetActive(true);

        yield return TextBox("There are additional things you can do. Check around the trebuchet again.");
        yield return TextBox("You can adjust the amount of weight inside the counterweight heavier and lighter to make the projectile fly further or shorter. It depends on your target.");
        yield return TextBox("And you can rotate the entire trebuchet left and right in fixed increments.");

        yield return TextBox("Feel free to experiment with everything here, because that's the end of weapons training.");
        
        yield return TextBox("Now that I've shown you how to fight, it's up to you to put it to the test in a real battle.");

        yield return EndTalk();

        // Allow player to shoot gun again
        playerController.canFireWeaponInHand = true;
    }

    /// <summary>
    /// Detects whether the character has...
    ///     <list type="bullet">
    ///         <item>Moved around in all 4 directions for enough seconds.</item>
    ///         <item>Looked around (any input) for enough seconds.</item>
    ///     </list>
    /// Also pulls up an info box on player's screen to show player if they've done the action or not for each of those two steps.
    /// </summary>
    /// <param name="charController"></param>
    protected IEnumerator WaitUntilCharacterMovesAndLooksAround(CharacterController charController)
    {
        // Reenable character movement
        charController.canInputMove = true;
        // Reenable character look so player doesn't think their mouse/look controls doesn't work
        charController.canInputLook = true;

        #region Wait until player moves around
        float timeWentForward = 0f;
        float timeWentBackward = 0f;
        float timeWentLeft = 0f;
        float timeWentRight = 0f;

        // How long player has to press each direction keys 
        float enoughSec = 0.65f;

        // Modify these strings to be displayed
        string fwdStr = "<color=white>Forward: --\n";
        string backStr = "<color=white>Backwd: --\n";
        string leftStr = "<color=white>Left: --\n";
        string rightStr = "<color=white>Right: --";

        // Concatenate the movement strings into one for info box
        string GetMovementInfoString()
        {
            return fwdStr + backStr + leftStr + rightStr;
        }

        // Check if timers are enough for each movement dir
        //  Timers are updated in while loop
        bool MovedForEnoughSec()
        {
            bool fwdOK = timeWentForward > enoughSec;
            bool backOK = timeWentBackward > enoughSec;
            bool leftOK = timeWentLeft > enoughSec;
            bool rightOK = timeWentRight > enoughSec;

            // Update the string if enough seconds
            if (fwdOK)
                fwdStr = "<color=white>Forward: <color=green>OK\n";
            if (backOK)
                backStr = "<color=white>Backwd: <color=green>OK\n";
            if (leftOK)
                leftStr = "<color=white>Left: <color=green>OK\n";
            if (rightOK)
                rightStr = "<color=white>Right: <color=green>OK\n";

            return fwdOK && backOK && leftOK && rightOK;
        }

        // Control an infobox on the screen
        InfoBox infoBox = dialogue.CreateInfoBox();
        infoBox.SetActive(true);

        // Initialize info box position, font sizing, starting text, ...
        infoBox.SetInfo(GetMovementInfoString());
        infoBox.SetInfoFontSize(50f);
        infoBox.SetWindowSize(300f, 315f);
        infoBox.SetWindowAnchorPosition(-600f, 100f);
        infoBox.SetVerticalFontAlignment(InfoBox.VerticalTextAlign.Middle);
        infoBox.SetHorizontalFontAlignment(InfoBox.HorizontalTextAlign.Right);

        // Wait until player presses each direction for enough seconds
        while (!MovedForEnoughSec())
        {
            Vector2 moveInput = charController.moveInput;
            float time = Time.deltaTime;

            // Right
            if (moveInput.x > 0)
                timeWentRight += time;
            // Left
            if (moveInput.x < 0)
                timeWentLeft += time;
            // Forward
            if (moveInput.y > 0)
                timeWentForward += time;
            // Backward
            if (moveInput.y < 0)
                timeWentBackward += time;

            // Did the player move at all?
            if (moveInput.magnitude > 0)
            {
                // Update dialogue window popup here
                infoBox.SetInfo(GetMovementInfoString());
            }

            yield return null;
        } 

        // Final update
        infoBox.SetInfo(GetMovementInfoString());
        #endregion

        // Gap between boxes
        const float gapBetweenBoxes = 2f;
        yield return new WaitForSeconds(gapBetweenBoxes);

        // Turn box off. (Maybe destroy it?)
        infoBox.SetActive(false);

        yield return new WaitForSeconds(gapBetweenBoxes);


        #region Look Around

        // Reenable character looking
        charController.canInputLook = true;

        string lookAroundStr = "Look Around: --\n";

        // Turn box on
        infoBox.SetActive(true);
        // Initialize info box position, font sizing, starting text, ...
        infoBox.SetInfo(lookAroundStr);
        infoBox.SetInfoFontSize(50f);
        infoBox.SetWindowSize(400f, 90f);
        infoBox.SetWindowAnchorPosition(-600f, 100f);
        infoBox.SetVerticalFontAlignment(InfoBox.VerticalTextAlign.Middle);
        infoBox.SetHorizontalFontAlignment(InfoBox.HorizontalTextAlign.Right);

        float timeLookAround = 0f;
        enoughSec = 0.75f;
        while(timeLookAround < enoughSec)
        {
            Vector2 lookInput = charController.lookInput;
            if (lookInput.magnitude > 0)
                timeLookAround += Time.deltaTime;
            yield return null;
        }

        // Player looked around enough, update info box and let player see for a couple sec
        infoBox.SetInfo("<color=white>Look Around: <color=green>OK\n");

        yield return new WaitForSeconds(gapBetweenBoxes);

        // Turn box off.
        infoBox.SetActive(false);

        yield return new WaitForSeconds(gapBetweenBoxes);

        #endregion
    }

    /// <summary>
    /// Detects when the character has jumped once + over the fence into the trigger.
    /// Has funny little dialogue bit to detect if got past fence without jumping.
    /// Has another dialogue bit to detect if player cannot jump past fence.
    /// </summary>
    protected IEnumerator WaitUntilCharacterJumpsOverFence(CharacterController charController, PlayerTrigger fenceTrigger)
    {
        // Reenable jumping for character
        charController.canInputJump = true;

        bool playerJumped = false;
        bool playerLanded = false;
        bool playerOverFence = false;

        bool playerSkippedFence = false;

        bool loopAgainSincePlayerJumpedButNotOverFence = false;

        // Keep track of how many times the player has failed the fence jump.
        int timesJumped = 0;

        #region Helper Functions
        void OnPlayerJump()
        {
            playerJumped = true;
            // Reset bool to wait for next land
            playerLanded = false;

            //timesJumped++;
        }
        void OnPlayerLand()
        {
            playerLanded = true;
        }
        void OnPlayerJumpedOverFence()
        {
            playerOverFence = true;
        }
        /// Main loop to detect if player has gotten over the fence yet.
        IEnumerator Loop()
        {
            // Restart loop bool, hope that this loop it will stay false
            loopAgainSincePlayerJumpedButNotOverFence = false;

            // Restart jump/land bools each loop
            playerJumped = false;
            playerLanded = false;
            playerOverFence = false;

            while (true)
            {
                // Player got past fence without jumping
                if (playerOverFence && !playerJumped)
                {
                    yield return TextBox("Did you just skip the fence without jumping?");
                    yield return TextBox("Well, the movement system isn't perfect. But it works for now.");
                    yield return TextBox("Anyways...");
                    playerSkippedFence = true;
                    break;
                }

                // Player in mid-air still
                else if (!playerLanded)
                {
                    // Wait
                    yield return null;
                }

                // Player jumped once and landed, check if over fence or not.
                else if (playerLanded)
                {
                    // Player jumped past fence; ok
                    if (playerOverFence)
                    {
                        break;
                    }
                    // Player jumped but not past fence yet, let them try again
                    else
                    {
                        // Since can't pass out stuff from IEnumerator
                        // Set this flag so it will loop again from beginning from main function
                        //  Want to still detect if player skipped fence without jumping.
                        loopAgainSincePlayerJumpedButNotOverFence = true;
                        break;
                    }
                }

                yield return null;
            }
        }
        #endregion

        charController.OnCharacterJump += OnPlayerJump;
        charController.OnCharacterLand += OnPlayerLand;
        fenceTrigger.OnPlayerEnter += OnPlayerJumpedOverFence;

        // Wait for player choice and give appropriate text dialogue
        /* 
         * If they jump, wait to see if trigger is hit or not.
         * 
         * 1. No hit: Jump no fence
         * 2. Hit: Jump over fence
         * 
         * If they don't jump but trigger is hit:
         *  3. Over fence, no jump (hmmmmge, funny)
         */

        /*
         * What is this looping method...
         * 
         * Goal:
         * Want to keep looping the correct dialogue, while keeping track of each player's jump.
         *  If player jumps and lands over fence: OK
         *  If player jumps but doesn't land over fence: Keeps count of how many times they do that
         *  If player somehow skips fence without jumping through any of the jumps: Be able to display that dialogue bit telling them they glitched the game.
         */
        yield return Loop();
        
        // Keep doing the loop 
        while (loopAgainSincePlayerJumpedButNotOverFence)
        {
            timesJumped++;

            // How many times jumped already?
            if(timesJumped < 3)
            {
                yield return TextBox("Now jump over that fence with [SPACEBAR].", waitCondition: Loop());
            }
            else if (timesJumped < 5)
                yield return TextBox("Just hold [SPACEBAR] and jump over the fence by additionally holding down the direction you want to move [W A S D].", waitCondition: Loop());
            else if (timesJumped < 9)
                yield return TextBox("Look at the fence, move forward with [W] and at the same time press [SPACEBAR]. Maybe your keyboard or controls are broken?", waitCondition: Loop());
            else if (timesJumped == 9)
                yield return TextBox("You've gotta be pulling my leg here. [SPACEBAR] + [W] while close enough to the fence.", waitCondition: Loop());
            else if (timesJumped < 12)
                yield return TextBox("[SPACEBAR] + [W] while close enough and looking at the fence.", waitCondition: Loop());
            else if(timesJumped >= 12)
            {
                // Delete fence
                fence.SetActive(false);
                // Do not set loop wait condition
                yield return TextBox($"That's it, I'm removing the fence. You failed the first fence jump {timesJumped}+ times. Are you happy now?");
                yield return TextBox("That's the only bit I had time to code in this game. Please don't waste your time looking for more. I promise.");
                yield return TextBox("Moving on from that...");
                // Do not loop again
                break;
            }
        }

        // Special ballpark of narrator being irritated and knows the player is doing this on purpose, but player successfully jumped over fence before narrator forces removal.
        if (timesJumped >= 9 && timesJumped < 12 && !playerSkippedFence)
        {
            yield return TextBox("Sheesh, finally.");
            yield return TextBox("Anyways, moving on from that...");
        }
        
        charController.OnCharacterJump -= OnPlayerJump;
        charController.OnCharacterLand -= OnPlayerLand;
        fenceTrigger.OnPlayerEnter -= OnPlayerJumpedOverFence;

        // Disable fence trigger
        fenceTrigger.Enable(false);

        yield break;
    }

    /// <summary>
    /// Detects when the character has run for some seconds.
    /// </summary>
    protected IEnumerator WaitUntilCharacterRuns(CharacterController charController)
    {
        // Reenable sprinting for character
        charController.canInputSprint = true;

        const float enoughSec = 2f;
        float runTimer = 0f;
        while(runTimer < enoughSec)
        {
            if (charController.isSprinting)
                runTimer += Time.deltaTime;

            yield return null;
        }
    }

    /// <summary>
    /// No character to look at; only toggle the cutscene on.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator StartTalk()
    {
        // Turn on cutscene bars.
        dialogue.ToggleCutsceneBars();

        isRunning = true;

        yield break;
    }

    /// <summary>
    /// Only some announcer, not an actual character; toggle cutscene bar off.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator EndTalk()
    {
        // Turn off cutscene bars.
        dialogue.ToggleCutsceneBars();

        isRunning = false;
        yield break;
    }
}
