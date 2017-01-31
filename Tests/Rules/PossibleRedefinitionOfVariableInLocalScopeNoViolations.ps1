#
# Examples stolen from issue 265
#
$a = 'Unchanged'
function foo {
    Write-Output "foo can read `$a : `$a = $a"
    $script:a = 'Changed from foo' 
    Write-Output "foo can change it's own copy of `$a: `$a = $a"
}
foo
Write-Output "After foo call: `$a = $a"

$x = 2
$y = 4
function doubleX {
    $script:x *= 2
}
doubleX
$x
function addY {
    $script:x = $x + $y
}
addY
$x

$LogPath = 'c:\temp\foo.log'
function InitLogging {
    $script:LogPath = '\\server\share\foo.log'  # User probably needs to change to $script:LogPath = ...
}