# Introduction #

Neon VidUtil Has the following components.

  * User Interface
  * Core
  * Plugins

# User Interface #

The Neon user interface code is very simple. It determines what operations the user wishes to perform and hands it off to the Core.

# Core #

The Core is responsible for using plugins to determine information about files and supported operations. There are two types of operations: conversions and processes. A conversion is between two different formats. A process is an operation performed on a file of a given format that results in the same format type.

# Plugins #

Plugins are used to actually do things. They are used to read information about files and perform operations. Plugins are required to perform any conversion.