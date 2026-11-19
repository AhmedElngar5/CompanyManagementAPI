$baseUrl = "http://localhost:5050"
$errors = 0
$passed = 0

function Test-Endpoint {
    param($name, $scriptBlock)
    try {
        & $scriptBlock
        Write-Host "[PASS] $name" -ForegroundColor Green
        $script:passed++
    } catch {
        Write-Host "[FAIL] $name : $($_.Exception.Message)" -ForegroundColor Red
        $script:errors++
    }
}

Write-Host "=== STARTING API VERIFICATION SUITE ==="

# 1. Create Department 1 (IT)
$dept1Id = $null
Test-Endpoint "Create Department (IT)" {
    $body = @{ name = "IT Department"; description = "Information Technology"; location = "Building A, Floor 3" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments" -Method POST -Body $body -ContentType "application/json"
    if (-not $res.id) { throw "No ID returned" }
    $script:dept1Id = $res.id
}

# 2. Create Department 2 (HR)
$dept2Id = $null
Test-Endpoint "Create Department (HR)" {
    $body = @{ name = "HR Department"; description = "Human Resources"; location = "Building B, Floor 1" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments" -Method POST -Body $body -ContentType "application/json"
    if (-not $res.id) { throw "No ID returned" }
    $script:dept2Id = $res.id
}

# 3. List All Departments
Test-Endpoint "List All Departments" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments" -Method GET
    if ($res.Count -lt 2) { throw "Expected at least 2 departments, got $($res.Count)" }
}

# 4. Get Department by ID
Test-Endpoint "Get Department by ID" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept1Id" -Method GET
    if ($res.id -ne $dept1Id) { throw "Mismatched ID" }
    if ($res.name -ne "IT Department") { throw "Mismatched Name" }
}

# 5. Update Department
Test-Endpoint "Update Department" {
    $body = @{ name = "IT Department Updated"; description = "IT Department - Updated"; location = "Building A, Floor 5" } | ConvertTo-Json
    $res = Invoke-WebRequest -Uri "$baseUrl/api/departments/$dept1Id" -Method PUT -Body $body -ContentType "application/json" -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

# 6. Create Employee 1 (Ahmed in IT)
$emp1Id = $null
Test-Endpoint "Create Employee (Ahmed in IT)" {
    $body = @{ fullName = "Ahmed Hassan"; email = "ahmed.hassan@company.com"; salary = 8500.00; hireDate = "2024-01-15T00:00:00"; departmentId = $dept1Id } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
    if (-not $res.id) { throw "No employee ID returned" }
    $script:emp1Id = $res.id
}

# 7. Create Employee 2 (Sara in IT)
$emp2Id = $null
Test-Endpoint "Create Employee (Sara in IT)" {
    $body = @{ fullName = "Sara Ali"; email = "sara.ali@company.com"; salary = 12000.50; hireDate = "2023-06-01T00:00:00"; departmentId = $dept1Id } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
    if (-not $res.id) { throw "No employee ID returned" }
    $script:emp2Id = $res.id
}

# 8. Create Employee 3 (Omar in HR)
$emp3Id = $null
Test-Endpoint "Create Employee (Omar in HR)" {
    $body = @{ fullName = "Omar Khaled"; email = "omar.khaled@company.com"; salary = 5000.00; hireDate = "2024-03-10T00:00:00"; departmentId = $dept2Id } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
    if (-not $res.id) { throw "No employee ID returned" }
    $script:emp3Id = $res.id
}

# 9. List All Employees
Test-Endpoint "List All Employees" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method GET
    if ($res.data.Count -ne 3) { throw "Expected 3 employees, got $($res.data.Count)" }
    if ($res.pagination.totalCount -ne 3) { throw "Pagination totalCount mismatch" }
}

# 10. Get Employee by ID
Test-Endpoint "Get Employee by ID with Department Name" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/$emp1Id" -Method GET
    if ($res.id -ne $emp1Id) { throw "Mismatched ID" }
    if (-not $res.departmentName) { throw "DepartmentName missing" }
}

# 11. Nested Resource Route: Get Employee in Department
Test-Endpoint "Nested Route: /api/departments/$dept1Id/employees/$emp1Id" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept1Id/employees/$emp1Id" -Method GET
    if ($res.id -ne $emp1Id) { throw "Mismatched employee ID" }
}

# 12. Nested Route: Employee in wrong department (expect 404)
Test-Endpoint "Nested Route: Employee in wrong department returns 404" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept2Id/employees/$emp1Id" -Method GET
        throw "Expected 404 but got success"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 404) { throw "Expected 404, got $status" }
    }
}

# 13. Search Employees without name
Test-Endpoint "Search Employees (No Name): /api/employees/search" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/search" -Method GET
    if ($res.Count -ne 3) { throw "Expected 3 employees, got $($res.Count)" }
}

# 14. Search Employees with name: /api/employees/search/ahmed
Test-Endpoint "Search Employees: /api/employees/search/ahmed" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/search/ahmed" -Method GET
    if ($res.Count -ne 1) { throw "Expected 1 employee, got $($res.Count)" }
    if ($res[0].fullName -notmatch "Ahmed") { throw "Expected Ahmed" }
}

# 15. Alpha Constraint: /api/employees/search/by-name/ahmed
Test-Endpoint "Alpha Constraint Valid: /api/employees/search/by-name/ahmed" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/search/by-name/ahmed" -Method GET
    if ($res.Count -ne 1) { throw "Expected 1 employee, got $($res.Count)" }
}

# 16. Alpha Constraint Invalid: /api/employees/search/by-name/123abc
Test-Endpoint "Alpha Constraint Invalid (123abc returns 404)" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/employees/search/by-name/123abc" -Method GET
        throw "Expected 404"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 404) { throw "Expected 404, got $status" }
    }
}

# 17. Decimal Constraint: /api/employees/salary/5000
Test-Endpoint "Decimal Constraint Valid: /api/employees/salary/5000" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/salary/5000" -Method GET
    if ($res.Count -ne 1) { throw "Expected 1 employee, got $($res.Count)" }
    if ($res[0].fullName -notmatch "Omar") { throw "Expected Omar" }
}

# 18. Decimal Constraint Invalid: /api/employees/salary/notanumber
Test-Endpoint "Decimal Constraint Invalid (notanumber returns 404)" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/employees/salary/notanumber" -Method GET
        throw "Expected 404"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 404) { throw "Expected 404, got $status" }
    }
}

# 19. Integer Constraint Invalid: /api/employees/abc
Test-Endpoint "Integer Constraint Invalid (/api/employees/abc returns 404)" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/employees/abc" -Method GET
        throw "Expected 404"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 404) { throw "Expected 404, got $status" }
    }
}

# 20. Filter by Department and Salary Range: /api/employees/filter
Test-Endpoint "Filter: departmentId=$dept1Id&minSalary=6000&maxSalary=10000" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/filter?departmentId=$dept1Id&minSalary=6000&maxSalary=10000" -Method GET
    if ($res.Count -ne 1) { throw "Expected 1 employee (Ahmed), got $($res.Count)" }
    if ($res[0].id -ne $emp1Id) { throw "Expected employee ID $emp1Id" }
}

# 21. Filter by Department Only: /api/employees/filter?departmentId=...
Test-Endpoint "Filter: departmentId=$dept1Id only" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees/filter?departmentId=$dept1Id" -Method GET
    if ($res.Count -ne 2) { throw "Expected 2 employees in IT, got $($res.Count)" }
}

# 22. Filter invalid salary range (min > max returns 400)
Test-Endpoint "Filter Invalid Range (minSalary > maxSalary returns 400)" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/employees/filter?minSalary=20000&maxSalary=5000" -Method GET
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 23. Pagination
Test-Endpoint "Pagination: pageNumber=1, pageSize=2" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees?pageNumber=1&pageSize=2" -Method GET
    if ($res.data.Count -ne 2) { throw "Expected 2 items, got $($res.data.Count)" }
    if ($res.pagination.currentPage -ne 1) { throw "Current page should be 1" }
    if (-not $res.pagination.hasNext) { throw "HasNext should be true" }
}

# 24. Sorting by salary ascending
Test-Endpoint "Sorting: sortBy=salary&sortOrder=asc" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees?sortBy=salary&sortOrder=asc" -Method GET
    $salaries = $res.data | ForEach-Object { [decimal]$_.salary }
    if ($salaries[0] -gt $salaries[1] -or $salaries[1] -gt $salaries[2]) { throw "Not sorted ascending: $($salaries -join ', ')" }
}

# 25. Sorting by salary descending
Test-Endpoint "Sorting: sortBy=salary&sortOrder=desc" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/employees?sortBy=salary&sortOrder=desc" -Method GET
    $salaries = $res.data | ForEach-Object { [decimal]$_.salary }
    if ($salaries[0] -lt $salaries[1] -or $salaries[1] -lt $salaries[2]) { throw "Not sorted descending: $($salaries -join ', ')" }
}

# 26. Validation: Invalid DepartmentId (999) -> 400
Test-Endpoint "Validation: Invalid DepartmentId returns 400" {
    try {
        $body = @{ fullName = "Bad Dept"; email = "bad.dept@company.com"; salary = 5000; departmentId = 9999 } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 27. Validation: Duplicate Email -> 400
Test-Endpoint "Validation: Duplicate Email returns 400" {
    try {
        $body = @{ fullName = "Dup Email"; email = "ahmed.hassan@company.com"; salary = 5000; departmentId = $dept1Id } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 28. Validation: Invalid Email format -> 400
Test-Endpoint "Validation: Invalid Email format returns 400" {
    try {
        $body = @{ fullName = "Invalid Email"; email = "not-an-email"; salary = 5000; departmentId = $dept1Id } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 29. Validation: Negative Salary -> 400
Test-Endpoint "Validation: Negative Salary returns 400" {
    try {
        $body = @{ fullName = "Neg Salary"; email = "neg.sal@company.com"; salary = -500; departmentId = $dept1Id } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 30. Validation: Zero Salary -> 400
Test-Endpoint "Validation: Zero Salary returns 400" {
    try {
        $body = @{ fullName = "Zero Salary"; email = "zero.sal@company.com"; salary = 0; departmentId = $dept1Id } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/employees" -Method POST -Body $body -ContentType "application/json"
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 31. Relationship: Department Includes Employees
Test-Endpoint "Relationship: Department includes employees array" {
    $res = Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept1Id" -Method GET
    if ($res.employees.Count -ne 2) { throw "Expected 2 employees in IT department, got $($res.employees.Count)" }
    if ($res.employeeCount -ne 2) { throw "Employee count mismatch" }
}

# 32. Relationship Restriction: Prevent deleting department with employees -> 400
Test-Endpoint "Relationship: Prevent deleting department containing employees returns 400" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept1Id" -Method DELETE
        throw "Expected 400"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 400) { throw "Expected 400, got $status" }
    }
}

# 33. Clean up: Delete Employees
Test-Endpoint "Delete Employee 1" {
    $res = Invoke-WebRequest -Uri "$baseUrl/api/employees/$emp1Id" -Method DELETE -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

Test-Endpoint "Delete Employee 2" {
    $res = Invoke-WebRequest -Uri "$baseUrl/api/employees/$emp2Id" -Method DELETE -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

# 34. Delete empty department -> 204
Test-Endpoint "Delete now-empty Department 1" {
    $res = Invoke-WebRequest -Uri "$baseUrl/api/departments/$dept1Id" -Method DELETE -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

# 35. Verify Department 1 is gone -> 404
Test-Endpoint "Verify Department 1 returns 404 after delete" {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/departments/$dept1Id" -Method GET
        throw "Expected 404"
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 404) { throw "Expected 404, got $status" }
    }
}

# 36. Clean up: Delete Employee 3 and Department 2
Test-Endpoint "Delete Employee 3" {
    $res = Invoke-WebRequest -Uri "$baseUrl/api/employees/$emp3Id" -Method DELETE -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

Test-Endpoint "Delete Department 2" {
    $res = Invoke-WebRequest -Uri "$baseUrl/api/departments/$dept2Id" -Method DELETE -UseBasicParsing
    if ($res.StatusCode -ne 204) { throw "Expected 204, got $($res.StatusCode)" }
}

Write-Host "========================================"
Write-Host "TESTS COMPLETED: Passed = $passed, Failed = $errors"
