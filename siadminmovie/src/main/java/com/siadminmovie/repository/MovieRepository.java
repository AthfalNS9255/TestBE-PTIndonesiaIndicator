package com.siadminmovie.repository;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import com.siadminmovie.model.Movie;


public interface MovieRepository extends JpaRepository<Movie, Integer> {
    Page<Movie> findByNameContainingIgnoreCase(String keyword, Pageable pageable);
}
