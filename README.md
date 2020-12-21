Lootboxes [![Build Status](https://travis-ci.org/Enerdy/KeyChanger.svg?branch=master)](https://travis-ci.org/Enerdy/KeyChanger)
==========

This is a modified version of Enerdy's KeyChanger plugin and renamed to Lootboxes, due to the 2 new features added to it. Server owners will now be able to further customize key exchanges in ways that resemble lootboxes:

-- "EnableLootboxMode": 

    - If true, upon exchanging a key, you'll receive a fixed amount of random items (instead of just one), defined by "ItemCountGiven" inside the config file.
    - If false, player will receive the whole content of the lootbox upon using the exchange command (AKA every single item ID from the used key).
    
-- "ItemCountGiven":

    - Sets the amount of items the lootbox will randomly hand out to the player. For example, if you define 3 item IDs for a Temple Key in the config file, and set ItemCountGiven = 10, when using a key to open the lootbox, you'll receive 10 random items from it.
    
Also, have in mind you can play with the lootboxes rates by repeating an Item ID inside the rewards list of any key.

NOTE: THIS VERSION WAS TRANSLATED TO SPANISH.

Documentation available at the [TShock Forums](http://tshock.co/xf/index.php?threads/1-16-ssc-keychangerssc.2528/).
